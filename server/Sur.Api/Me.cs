namespace Sur.Api;

static partial class Fn
{
    public static UserItem Me(Domain domain) => domain.State.Users.First(u => u.Id == domain.State.CurrentUserId);
}
