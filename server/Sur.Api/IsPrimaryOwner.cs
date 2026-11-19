namespace Sur.Api;

static partial class Fn
{
    public static bool IsPrimaryOwner(Domain domain, RiskItem risk) => IsAdmin(domain) || PrimaryLink(risk).OwnerId == Me(domain).Id;
}
