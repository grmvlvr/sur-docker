namespace Sur.Api;

static partial class Fn
{
    public static TreatmentItem? FindTreatment(Domain domain, string id) => domain.State.Treatments.FirstOrDefault(t => t.Id == id);
}
