namespace Sur.Api;

static partial class Fn
{
    public static Result SaveRole(Domain domain, string code, RoleInput input)
    {
        if (!IsAdmin(domain)) return Fail("Роли меняет администратор.");
        var role = domain.State.Roles.FirstOrDefault(item => item.Code == code);
        if (role == null) return Fail("Роль не найдена.");
        if (code == "ADMIN" && !input.IsActive) return Fail("Роль администратора нельзя деактивировать.");
        if (code == "ADMIN" && !input.Rights.Contains("admin")) return Fail("Право admin нельзя снять с администратора.");
        role.IsActive = input.IsActive;
        role.Rights = input.Rights.Distinct().ToList();
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
