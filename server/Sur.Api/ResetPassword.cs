namespace Sur.Api;

static partial class Fn
{
    public static Result ResetPassword(Domain domain, string userId)
    {
        if (!IsAdmin(domain)) return Fail("Пароль сбрасывает администратор.");
        var user = domain.State.Users.FirstOrDefault(item => item.Id == userId);
        if (user == null) return Fail("Пользователь не найден.");
        user.PasswordHash = HashPassword(DemoPassword);
        var saved = SaveState(domain);
        return saved.Ok
            ? new Result(true, $"Пароль сброшен на {DemoPassword}. Письмо в прототипе не отправляется.", userId)
            : saved;
    }
}
