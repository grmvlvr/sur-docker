using System.Security.Cryptography;

namespace Sur.Api;

static partial class Fn
{
    public static Result Login(Domain domain, string login, string password)
    {
        var key = login.Trim();
        var user = domain.State.Users.FirstOrDefault(item =>
            string.Equals(item.Login, key, StringComparison.OrdinalIgnoreCase)
            || string.Equals(item.Login.Split('@')[0], key, StringComparison.OrdinalIgnoreCase));
        if (user is not { IsActive: true } || !PasswordMatches(password, user.PasswordHash))
            return Fail("Неверный email или пароль.");
        domain.State.CurrentUserId = user.Id;
        domain.State.LoggedIn = true;
        var saved = SaveState(domain);
        return saved.Ok ? Ok(user.Id) : saved;
    }

    public static bool PasswordMatches(string password, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 2) return false;
        try
        {
            var salt = Convert.FromBase64String(parts[0]);
            var expected = Convert.FromBase64String(parts[1]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
