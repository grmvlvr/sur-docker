namespace Sur.Api;

static partial class Fn
{
    public static Result Ok(string? id = null) => new(true, null, id);
}
