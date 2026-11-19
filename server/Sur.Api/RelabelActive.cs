namespace Sur.Api;

static partial class Fn
{
    public static void RelabelActive(Domain domain)
    {
        foreach (var risk in domain.State.Risks.Where(item => item.Status == "Активен" && item.Assessment != null))
            risk.AcceptanceLevel = AcceptanceLevel(domain, risk.Assessment!.Score);
    }
}
