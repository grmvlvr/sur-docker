namespace Sur.Api;

static partial class Fn
{
    public static List<string> EffectiveRights(Domain domain)
    {
        if (IsAdmin(domain)) return AllRights();
        var codes = Me(domain).Roles;
        return domain.State.Roles
            .Where(role => role.IsActive && codes.Contains(role.Code))
            .SelectMany(role => role.Rights)
            .Distinct()
            .OrderBy(right => right)
            .ToList();
    }

    public static List<string> AllRights() =>
        RoleRights.Map.Values
            .SelectMany(rights => rights)
            .Append("admin")
            .Distinct()
            .OrderBy(right => right)
            .ToList();
}
