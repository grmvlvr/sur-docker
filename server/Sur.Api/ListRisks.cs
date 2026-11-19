namespace Sur.Api;

static partial class Fn
{
    public static IResult ListRisks(Domain domain)
    {
        if (!HasRight(domain, "view_risk"))
            return Results.Json(new { ok = false, message = "Нет права на просмотр рисков." }, statusCode: 403);
        return Results.Ok(domain.State.Risks);
    }
}
