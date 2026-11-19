namespace Sur.Api;

static partial class Fn
{
    public static string? ApprovalLevel(Domain domain, decimal cost)
    {
        var thresholds = domain.State.Thresholds;
        if (cost < thresholds.OwnerCost) return null;
        if (cost >= thresholds.CeoCost) return "CEO";
        if (cost >= thresholds.DirectorCost) return "DIRECTOR";
        return "OWNER";
    }
}
