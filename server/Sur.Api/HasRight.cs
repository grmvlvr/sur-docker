namespace Sur.Api;

static partial class Fn
{
    public static bool HasRight(Domain domain, string right)
    {
        var codes = Me(domain).Roles;
        if (codes.Contains("ADMIN")) return true;
        var roles = domain.State.Roles.Where(role => role.IsActive && codes.Contains(role.Code));
        return roles.Any(role => role.Rights.Contains(right));
    }
}
