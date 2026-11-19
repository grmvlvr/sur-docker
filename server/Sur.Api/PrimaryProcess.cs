namespace Sur.Api;

static partial class Fn
{
    public static ProcessItem PrimaryProcess(Domain domain, RiskItem risk) => FindProcess(domain, PrimaryLink(risk).ProcessId)!;
}
