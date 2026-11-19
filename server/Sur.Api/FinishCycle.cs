namespace Sur.Api;

static partial class Fn
{
    public static void FinishCycle(Domain domain, RiskItem risk)
    {
        if (risk.Status != "Активен" || risk.Assessment == null) return;
        if (TreatmentsOf(domain, risk.Id).Any(item => Rules.OpenTreatment.Contains(item.Status))) return;
        risk.History.Insert(0, risk.Assessment);
        risk.Assessment.IsActive = false;
        risk.Assessment = null;
        risk.AcceptanceLevel = null;
        risk.Status = "Ожидает переоценки";
        AppendLog(domain, risk.Id, "Риск ожидает переоценки после завершения мер");
    }
}
