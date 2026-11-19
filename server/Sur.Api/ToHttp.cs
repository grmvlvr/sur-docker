namespace Sur.Api;

static partial class Fn
{
    public static IResult ToHttp(Result result) =>
        result.Ok
            ? Results.Ok(new { ok = true, id = result.Id, message = result.Message })
            : Results.BadRequest(new { ok = false, message = result.Message });
}
