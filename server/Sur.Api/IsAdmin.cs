namespace Sur.Api;

static partial class Fn
{
    public static bool IsAdmin(Domain domain) => Me(domain).Roles.Contains("ADMIN");
}
