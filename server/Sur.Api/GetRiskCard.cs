namespace Sur.Api;

static partial class Fn
{
    public static IResult GetRiskCard(Domain domain, string id)
    {
        if (!HasRight(domain, "view_risk"))
            return Results.Json(new { ok = false, message = "Нет права на просмотр риска." }, statusCode: 403);
        var risk = FindRisk(domain, id);
        if (risk == null) return Results.NotFound();
        return Results.Ok(new
        {
            risk,
            treatments = domain.State.Treatments.Where(t => t.RiskId == id),
            comments = domain.State.Comments.Where(c => c.RiskId == id && !c.IsDeleted),
            log = domain.State.Log.Where(l => l.RiskId == id)
        });
    }
}
