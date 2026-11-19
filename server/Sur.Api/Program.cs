using Sur.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<Domain>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));
var app = builder.Build();
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path.Value ?? "";
    if (!path.StartsWith("/api", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/api/login", StringComparison.OrdinalIgnoreCase))
    {
        await next();
        return;
    }
    var domain = ctx.RequestServices.GetRequiredService<Domain>();
    if (!domain.State.LoggedIn)
    {
        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await ctx.Response.WriteAsJsonAsync(new { ok = false, message = "Войдите в систему." });
        return;
    }
    await next();
});

app.MapGet("/api/meta", (Domain d) => Fn.GetMeta(d));
app.MapPost("/api/login", (Domain d, LoginInput body) => Fn.ToHttp(Fn.Login(d, body.Login, body.Password)));
app.MapPost("/api/logout", (Domain d) => Fn.ToHttp(Fn.Logout(d)));
app.MapGet("/api/risks", (Domain d) => Fn.ListRisks(d));
app.MapGet("/api/risks/{id}", (Domain d, string id) => Fn.GetRiskCard(d, id));
app.MapGet("/api/treatments", (Domain d) => Fn.ListTreatments(d));
app.MapPost("/api/risks", (Domain d, RiskInput body) => Fn.ToHttp(Fn.CreateRisk(d, body)));
app.MapPut("/api/risks/{id}", (Domain d, string id, RiskInput body) => Fn.ToHttp(Fn.EditPassport(d, id, body)));
app.MapPost("/api/risks/{id}/assess", (Domain d, string id, AssessInput body) => Fn.ToHttp(Fn.Assess(d, id, body)));
app.MapPost("/api/risks/{id}/mode", (Domain d, string id, ModeInput body) => Fn.ToHttp(Fn.SetMode(d, id, body)));
app.MapPost("/api/risks/{id}/close", (Domain d, string id, CloseInput body) => Fn.ToHttp(Fn.CloseRisk(d, id, body.Reason)));
app.MapPost("/api/risks/{id}/reopen", (Domain d, string id) => Fn.ToHttp(Fn.ReopenRisk(d, id)));
app.MapPost("/api/risks/{id}/comments", (Domain d, string id, CommentInput body) => Fn.ToHttp(Fn.AddComment(d, id, body.Text)));
app.MapPost("/api/comments/{id}", (Domain d, string id, CommentInput body) => Fn.ToHttp(Fn.EditComment(d, id, body.Text)));
app.MapPost("/api/comments/{id}/delete", (Domain d, string id) => Fn.ToHttp(Fn.DeleteComment(d, id)));
app.MapPost("/api/risks/{id}/treatments", (Domain d, string id, TreatmentInput body) => Fn.ToHttp(Fn.CreateTreatment(d, id, body)));
app.MapPost("/api/treatments/{id}/approval", (Domain d, string id) => Fn.ToHttp(Fn.SendApproval(d, id)));
app.MapPost("/api/treatments/{id}/decision", (Domain d, string id, Decision body) => Fn.ToHttp(Fn.DecideTreatment(d, id, body.Approve, body.Comment)));
app.MapPost("/api/treatments/{id}/start", (Domain d, string id, StartInput body) => Fn.ToHttp(Fn.StartTreatment(d, id, body.ExecutorId, body.FailMock)));
app.MapPost("/api/treatments/{id}/mock", (Domain d, string id, StartInput body) => Fn.ToHttp(Fn.RetryMock(d, id, body.FailMock)));
app.MapPost("/api/treatments/{id}/work", (Domain d, string id) => Fn.ToHttp(Fn.BeginWork(d, id)));
app.MapPost("/api/treatments/{id}/jira-done", (Domain d, string id, JiraInput body) => Fn.ToHttp(Fn.JiraDone(d, id, body.CostActual)));
app.MapPost("/api/treatments/{id}/cancel", (Domain d, string id) => Fn.ToHttp(Fn.CancelTreatment(d, id)));
app.MapPut("/api/treatments/{id}", (Domain d, string id, TreatmentInput body) => Fn.ToHttp(Fn.EditTreatment(d, id, body)));
app.MapPost("/api/treatments/{id}/hold", (Domain d, string id) => Fn.ToHttp(Fn.HoldTreatment(d, id)));
app.MapPost("/api/treatments/{id}/resume", (Domain d, string id) => Fn.ToHttp(Fn.ResumeTreatment(d, id)));
app.MapPost("/api/treatments/{id}/executor", (Domain d, string id, StartInput body) => Fn.ToHttp(Fn.ReplaceExecutor(d, id, body.ExecutorId)));
app.MapPost("/api/treatments/{id}/executor/clear", (Domain d, string id) => Fn.ToHttp(Fn.ClearExecutor(d, id)));
app.MapGet("/api/approvals", (Domain d) => Fn.ListApprovals(d));
app.MapPost("/api/approvals/{id}/revoke", (Domain d, string id) => Fn.ToHttp(Fn.RevokeApproval(d, id)));
app.MapPut("/api/scales", (Domain d, ScaleInput body) => Fn.ToHttp(Fn.SaveScales(d, body)));
app.MapPut("/api/thresholds", (Domain d, ThresholdInput body) => Fn.ToHttp(Fn.SaveThresholds(d, body)));
app.MapPut("/api/roles/{code}", (Domain d, string code, RoleInput body) => Fn.ToHttp(Fn.SaveRole(d, code, body)));
app.MapPost("/api/users", (Domain d, UserInput body) => Fn.ToHttp(Fn.SaveUser(d, null, body)));
app.MapPut("/api/users/{id}", (Domain d, string id, UserInput body) => Fn.ToHttp(Fn.SaveUser(d, id, body)));
app.MapPost("/api/users/{id}/password-reset", (Domain d, string id) => Fn.ToHttp(Fn.ResetPassword(d, id)));
app.MapPost("/api/directorates", (Domain d, NamedInput body) => Fn.ToHttp(Fn.SaveDirectorate(d, null, body)));
app.MapPut("/api/directorates/{id}", (Domain d, string id, NamedInput body) => Fn.ToHttp(Fn.SaveDirectorate(d, id, body)));
app.MapPost("/api/processes", (Domain d, ProcessInput body) => Fn.ToHttp(Fn.SaveProcess(d, null, body)));
app.MapPut("/api/processes/{id}", (Domain d, string id, ProcessInput body) => Fn.ToHttp(Fn.SaveProcess(d, id, body)));
app.MapPost("/api/objects", (Domain d, ObjectInput body) => Fn.ToHttp(Fn.SaveObject(d, null, body)));
app.MapPut("/api/objects/{id}", (Domain d, string id, ObjectInput body) => Fn.ToHttp(Fn.SaveObject(d, id, body)));
app.MapPost("/api/categories", (Domain d, NamedInput body) => Fn.ToHttp(Fn.SaveCategory(d, null, body)));
app.MapPut("/api/categories/{id}", (Domain d, string id, NamedInput body) => Fn.ToHttp(Fn.SaveCategory(d, id, body)));
app.MapPost("/api/hazards/{id}/active", (Domain d, string id, NamedInput body) => Fn.ToHttp(Fn.ToggleHazard(d, id, body.IsActive)));
app.MapPost("/api/hazards", (Domain d, HazardInput body) => Fn.ToHttp(Fn.AddHazard(d, body.Name)));
app.MapPost("/api/import/risks", (Domain d, ImportInput body) => Fn.ImportRisks(d, body.Csv));
app.MapPost("/api/risks/{id}/snapshot", (Domain d, string id) => Fn.CreateSnapshot(d, id));
app.MapGet("/api/reports/snapshots/{id}/csv", (Domain d, string id) => Fn.OpenSnapshot(d, id, "csv"));
app.MapGet("/api/reports/snapshots/{id}/pdf", (Domain d, string id) => Fn.OpenSnapshot(d, id, "pdf"));

app.MapFallbackToFile("index.html");
app.Run();
