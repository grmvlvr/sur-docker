namespace Sur.Api;

static partial class Fn
{
    public static void RecalcActive(Domain domain)
    {
        foreach (var risk in domain.State.Risks.Where(item => item.Status == "Активен" && item.Assessment is { IsActive: true }).ToList())
        {
            var current = risk.Assessment!;
            var impact = ImpactOf(domain, current.LossAmount);
            var score = current.Likelihood * impact;
            current.IsActive = false;
            risk.History.Insert(0, current);
            var level = AcceptanceLevel(domain, score);
            risk.Assessment = CopyAssessment(current, impact, score, level, current.LossAmount, current.HourlyDowntimeCost);
            risk.Assessment.At = Stamp();
            risk.AcceptanceLevel = level;
            AppendLog(domain, risk.Id, "Технический пересчёт оценки после изменения шкалы");
        }
    }
}
