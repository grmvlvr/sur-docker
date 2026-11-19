namespace Sur.Api;

static partial class Fn
{
    public static object GetMeta(Domain domain)
    {
        var admin = IsAdmin(domain);
        return new
        {
            me = PublicUser(Me(domain), EffectiveRights(domain)),
            users = domain.State.Users.Select(user => PublicUser(user, null)),
            directorates = domain.State.Directorates,
            processes = domain.State.Processes,
            objects = domain.State.Objects,
            hazards = domain.State.Hazards,
            categories = domain.State.Categories,
            roles = domain.State.Roles.Select(role => new
            {
                role.Code,
                role.Name,
                role.IsActive,
                Rights = admin ? role.Rights : new List<string>()
            }),
            rightCatalog = admin ? AllRights() : new List<string>(),
            thresholds = domain.State.Thresholds,
            scales = Scales(domain)
        };
    }

    static object PublicUser(UserItem user, List<string>? rights) => new
    {
        user.Id,
        user.Name,
        user.Login,
        user.Roles,
        user.DirectorateId,
        user.IsActive,
        rights
    };
}
