namespace Sur.Api;

static partial class Fn
{
    public static IResult ListTreatments(Domain domain)
    {
        if (!HasRight(domain, "view_risk"))
            return Results.Json(new { ok = false, message = "Нет права на просмотр мер." }, statusCode: 403);
        return Results.Ok(domain.State.Treatments);
    }
}
