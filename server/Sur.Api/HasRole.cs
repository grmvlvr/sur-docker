namespace Sur.Api;

static partial class Fn
{
    public static bool HasRole(Domain domain, string role) => IsAdmin(domain) || Me(domain).Roles.Contains(role);
}
