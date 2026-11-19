namespace Sur.Api;

static partial class Fn
{
    public static RiskProcess PrimaryLink(RiskItem risk) => risk.Processes.First(p => p.IsPrimary && string.IsNullOrEmpty(p.UnlinkedAt));
}
