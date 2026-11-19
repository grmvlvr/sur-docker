namespace Sur.Api;

static partial class Fn
{
    public static Result Logout(Domain domain)
    {
        domain.State.LoggedIn = false;
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
