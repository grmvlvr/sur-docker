namespace Sur.Api;

static partial class Fn
{
    public static Result SaveUser(Domain domain, string? id, UserInput input)
    {
        if (!IsAdmin(domain)) return Fail("Пользователей меняет администратор.");
        if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Login)) return Fail("Укажите ФИО и email.");
        var email = input.Login.Trim().ToLowerInvariant();
        if (!email.Contains('@')) return Fail("Email используется как логин для входа.");
        if (domain.State.Users.Any(item => item.Id != id && string.Equals(item.Login, email, StringComparison.OrdinalIgnoreCase)))
            return Fail("Такой email уже занят.");
        if (!string.IsNullOrEmpty(id) && !input.IsActive && domain.State.Treatments.Any(item => item.ExecutorId == id && item.Status is "Ожидает выполнения" or "В работе"))
            return Fail("Нельзя деактивировать исполнителя меры в ожидании или в работе.");
        if (string.IsNullOrEmpty(id))
        {
            var newId = "u" + ++domain.State.Seq;
            domain.State.Users.Add(new UserItem
            {
                Id = newId,
                Name = input.Name.Trim(),
                Login = email,
                Roles = input.Roles,
                DirectorateId = input.DirectorateId,
                IsActive = input.IsActive,
                PasswordHash = HashPassword(DemoPassword)
            });
            var savedNew = SaveState(domain);
            return savedNew.Ok
                ? new Result(true, $"Пользователь создан. Временный пароль: {DemoPassword}. Письмо в прототипе не отправляется.", newId)
                : savedNew;
        }

        var user = domain.State.Users.FirstOrDefault(item => item.Id == id);
        if (user == null) return Fail("Пользователь не найден.");
        user.Name = input.Name.Trim();
        user.Login = email;
        user.Roles = input.Roles;
        user.DirectorateId = input.DirectorateId;
        user.IsActive = input.IsActive;
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
