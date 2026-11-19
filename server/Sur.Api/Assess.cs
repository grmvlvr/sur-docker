namespace Sur.Api;

static partial class Fn
{
    public static Result Assess(Domain domain, string id, AssessInput input)
    {
        var risk = FindRisk(domain, id);
        if (risk == null) return Fail("Риск не найден.");
        if (risk.Status is not ("Черновик" or "Ожидает переоценки"))
            return Fail("Оценка доступна для «Черновика» и «Ожидает переоценки».");
        if (risk.Status == "Черновик" && risk.Assessment != null) return Fail("Первичная оценка уже есть.");
        if (risk.Status == "Ожидает переоценки" && risk.Assessment is { IsActive: true })
            return Fail("Сначала должна отсутствовать текущая оценка.");
        if (!IsPrimaryOwner(domain, risk)) return Fail("Оценивает владелец основного процесса.");
        if (risk.Status == "Черновик" && (!HasRight(domain, "assess_risk") || !HasRight(domain, "edit_risk")))
            return Fail("Для первичной оценки нужны права оценки и редактирования.");
        if (risk.Status == "Ожидает переоценки" && !HasRight(domain, "assess_risk"))
            return Fail("Для переоценки нужно право оценки.");
        if (!Rules.Likelihoods.Contains(input.Likelihood)) return Fail("Выберите вероятность от 1 до 5.");
        decimal loss;
        decimal? downtime = null;
        decimal? downtimeTotal = null;
        int? weeks = null;
        int? days = null;
        decimal? hourly = null;
        if (risk.ObjectId != null)
        {
            weeks = input.DowntimeWeeks ?? 0;
            days = input.DowntimeDays ?? 0;
            downtime = input.DowntimeHours ?? 0;
            if (weeks < 0 || days < 0 || downtime < 0) return Fail("Простой не может быть отрицательным.");
            var asset = domain.State.Objects.First(item => item.Id == risk.ObjectId);
            if (asset.Hourly < 0) return Fail("У объекта не заполнена стоимость часа простоя.");
            hourly = asset.Hourly;
            var total = weeks.Value * 168m + days.Value * 24m + downtime.Value;
            loss = Math.Round(total * asset.Hourly, 2, MidpointRounding.AwayFromZero);
            downtimeTotal = total;
        }
        else
        {
            if (input.LossAmount is not >= 0) return Fail("Укажите сумму последствий.");
            loss = input.LossAmount.Value;
        }
        var impact = ImpactOf(domain, loss);
        var score = input.Likelihood * impact;
        if (risk.Assessment != null)
        {
            risk.Assessment.IsActive = false;
            risk.History.Insert(0, risk.Assessment);
        }
        var level = AcceptanceLevel(domain, score);
        risk.Assessment = new Assessment
        {
            Likelihood = input.Likelihood,
            LossAmount = loss,
            DowntimeWeeks = weeks,
            DowntimeDays = days,
            DowntimeHours = downtime,
            DowntimeTotalHours = downtimeTotal,
            HourlyDowntimeCost = hourly,
            Impact = impact,
            Score = score,
            Level = level,
            ResponseMode = null,
            IsActive = true,
            At = Stamp(),
            AuthorId = Me(domain).Id
        };
        risk.AcceptanceLevel = level;
        risk.Status = "Активен";
        AppendLog(domain, id, $"Оценка {input.Likelihood}×{impact} = {score}, уровень {Rules.Levels[risk.Assessment.Level]}");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
