namespace Sur.Api;

static partial class Fn
{
    public static bool IsTreatmentOwner(Domain domain, TreatmentItem treatment)
    {
        var risk = FindRisk(domain, treatment.RiskId)!;
        return IsAdmin(domain) || risk.Processes.Any(p => p.ProcessId == treatment.ProcessId && p.OwnerId == Me(domain).Id);
    }
}
