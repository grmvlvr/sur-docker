namespace Sur.Api;

static partial class Fn
{
    public static IResult OpenSnapshot(Domain domain, string id, string kind)
    {
        if (!HasRight(domain, "view_report") && !HasRight(domain, "export_report"))
            return Results.Json(new { ok = false, message = "Нет права на отчёт." }, statusCode: 403);
        var snapshot = domain.State.Snapshots.FirstOrDefault(item => item.Id == id);
        if (snapshot is not { Status: "Готов" }) return Results.BadRequest(new { ok = false, message = "Готовый снимок не найден." });
        var ext = kind == "pdf" ? "pdf" : "csv";
        var path = Path.Combine(Path.GetDirectoryName(domain.StorePath)!, "snapshots", id + "." + ext);
        if (!File.Exists(path)) return Results.BadRequest(new { ok = false, message = "Файл снимка не найден." });
        var type = ext == "pdf" ? "application/pdf" : "text/csv";
        return Results.File(File.ReadAllBytes(path), type, "audit-snapshot-" + id + "." + ext);
    }
}
