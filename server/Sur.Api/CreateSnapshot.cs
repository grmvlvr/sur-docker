namespace Sur.Api;

static partial class Fn
{
    public static IResult CreateSnapshot(Domain domain, string riskId)
    {
        if (!HasRight(domain, "view_report") && !HasRight(domain, "export_report"))
            return Results.Json(new { ok = false, message = "Нет права на формирование отчёта." }, statusCode: 403);
        var risk = FindRisk(domain, riskId);
        if (risk == null) return Results.BadRequest(new { ok = false, message = "Риск не найден." });
        var id = "s" + ++domain.State.Seq;
        var at = Stamp();
        var author = Me(domain);
        var parts = SnapshotParts(domain, risk, id, at, author);
        var pdf = WritePdf(parts, id);
        if (pdf == null)
        {
            domain.State.Snapshots.Add(new SnapshotItem { Id = id, RiskId = riskId, GeneratedAt = at, AuthorId = author.Id, Status = "Ошибка", Error = "PDF не сформирован." });
            SaveState(domain);
            return Results.BadRequest(new { ok = false, message = "PDF не сформирован. Готовые файлы не выданы." });
        }

        var lines = SnapshotLines(parts);
        var csv = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(string.Join("\n", lines))).ToArray();
        var dir = Path.Combine(Path.GetDirectoryName(domain.StorePath)!, "snapshots");
        Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, id + ".csv"), csv);
        File.WriteAllBytes(Path.Combine(dir, id + ".pdf"), pdf);
        domain.State.Snapshots.Add(new SnapshotItem { Id = id, RiskId = riskId, GeneratedAt = at, AuthorId = author.Id, Status = "Готов" });
        var saved = SaveState(domain);
        if (!saved.Ok) return Results.BadRequest(new { ok = false, message = saved.Message });
        return Results.Ok(new
        {
            ok = true,
            snapshotId = id,
            csv = $"/api/reports/snapshots/{id}/csv",
            pdf = $"/api/reports/snapshots/{id}/pdf"
        });
    }
}
