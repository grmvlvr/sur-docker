namespace Sur.Api;

static partial class Fn
{
    public static Result Fail(string message) => new(false, message, null);
}
