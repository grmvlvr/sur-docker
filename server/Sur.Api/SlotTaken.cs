namespace Sur.Api;

static partial class Fn
{
    public static bool SlotTaken(Domain domain, string riskId, string directorateId, string exceptTreatmentId) =>
        domain.State.Treatments.Any(item =>
            item.Id != exceptTreatmentId
            && item.RiskId == riskId
            && Rules.LimitingTreatment.Contains(item.Status)
            && FindProcess(domain, item.ProcessId)?.DirectorateId == directorateId);
}
