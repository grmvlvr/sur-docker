namespace Sur.Api;

static partial class Fn
{
    public static void RecalcHourly(Domain domain, string objectId, decimal hourly)
    {
        foreach (var risk in domain.State.Risks.Where(item => item.ObjectId == objectId && item.Status is not ("Принят" or "Закрыт") && item.Assessment is { IsActive: true }).ToList())
        {
            var current = risk.Assessment!;
            var total = current.DowntimeTotalHours ?? current.DowntimeHours ?? 0;
            var loss = Math.Round(total * hourly, 2, MidpointRounding.AwayFromZero);
            var impact = ImpactOf(domain, loss);
            var score = current.Likelihood * impact;
            current.IsActive = false;
            risk.History.Insert(0, current);
            var level = AcceptanceLevel(domain, score);
            risk.Assessment = CopyAssessment(current, impact, score, level, loss, hourly);
            risk.Assessment.At = Stamp();
            risk.AcceptanceLevel = level;
            AppendLog(domain, risk.Id, "Технический пересчёт простоя после изменения стоимости часа");
        }
    }
}
