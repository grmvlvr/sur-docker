namespace Sur.Api;

static partial class Fn
{
    public static List<TreatmentItem> TreatmentsOf(Domain domain, string riskId) => domain.State.Treatments.Where(t => t.RiskId == riskId).ToList();
}
