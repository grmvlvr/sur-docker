namespace Sur.Api;

static partial class Fn
{
    public static RiskItem? FindRisk(Domain domain, string id) => domain.State.Risks.FirstOrDefault(r => r.Id == id);
}
