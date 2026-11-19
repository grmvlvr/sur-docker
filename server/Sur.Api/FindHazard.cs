namespace Sur.Api;

static partial class Fn
{
    public static HazardItem? FindHazard(Domain domain, string id) => domain.State.Hazards.FirstOrDefault(h => h.Id == id);
}
