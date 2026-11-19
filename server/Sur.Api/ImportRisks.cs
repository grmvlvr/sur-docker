namespace Sur.Api;

static partial class Fn
{
    public static IResult ImportRisks(Domain domain, string? csv)
    {
        if (!HasRight(domain, "create_risk")) return Results.Json(new { ok = false, message = "Импорт доступен пользователю с правом создавать риск." }, statusCode: 403);
        if (string.IsNullOrWhiteSpace(csv)) return Results.BadRequest(new { ok = false, message = "Файл пуст." });
        var lines = csv.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var header = lines[0].Trim().TrimStart('\uFEFF');
        var expected = new[] { "sourceRiskKey", "title", "description", "hazard", "category", "primaryProcess", "additionalProcesses" };
        var columns = header.Split(';');
        var missing = expected.Where(name => !columns.Contains(name)).ToArray();
        if (missing.Length > 0 || columns.Length < expected.Length)
            return Results.BadRequest(new { ok = false, message = "Не хватает колонок: " + string.Join(", ", missing.Length == 0 ? expected : missing) + ". Разделитель — точка с запятой." });

        var index = expected.ToDictionary(name => name, name => Array.IndexOf(columns, name));
        var errors = new List<object>();
        var created = 0;
        var skipped = 0;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var line = 1; line < lines.Length; line++)
        {
            if (string.IsNullOrWhiteSpace(lines[line])) continue;
            var cells = lines[line].Split(';');
            string Cell(string name) => index[name] < cells.Length ? cells[index[name]].Trim() : "";
            var key = Cell("sourceRiskKey");
            var row = line + 1;
            if (key.Length == 0)
            {
                errors.Add(new { line = row, field = "sourceRiskKey", message = "Пустой внешний ключ." });
                continue;
            }
            if (!seen.Add(key) || domain.State.Risks.Any(item => string.Equals(item.SourceKey, key, StringComparison.OrdinalIgnoreCase)))
            {
                skipped++;
                continue;
            }
            var title = Cell("title");
            var description = Cell("description");
            if (title.Length == 0 || description.Length == 0)
            {
                errors.Add(new { line = row, field = "title", message = "Нужны название и описание." });
                continue;
            }
            var hazard = ByName(domain.State.Hazards, Cell("hazard"));
            if (hazard == null)
            {
                errors.Add(new { line = row, field = "hazard", message = "Активная опасность не найдена." });
                continue;
            }
            var category = ByName(domain.State.Categories, Cell("category"));
            if (category == null)
            {
                errors.Add(new { line = row, field = "category", message = "Активная категория не найдена." });
                continue;
            }
            var primaryName = Cell("primaryProcess");
            var primary = ByName(domain.State.Processes, primaryName);
            if (primary == null)
            {
                errors.Add(new { line = row, field = "primaryProcess", message = "Активный основной процесс не найден." });
                continue;
            }
            var extraNames = Cell("additionalProcesses").Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (extraNames.Any(name => string.Equals(name, primaryName, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add(new { line = row, field = "additionalProcesses", message = "Основной процесс повторен в дополнительных." });
                continue;
            }
            var extras = new List<ProcessItem>();
            var extraFailed = false;
            foreach (var name in extraNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var process = ByName(domain.State.Processes, name);
                if (process == null)
                {
                    errors.Add(new { line = row, field = "additionalProcesses", message = $"Активный процесс «{name}» не найден." });
                    extraFailed = true;
                    break;
                }
                extras.Add(process);
            }
            if (extraFailed) continue;
            var id = (246000 + ++domain.State.Seq).ToString();
            var now = Stamp();
            var links = new List<RiskProcess> { new() { ProcessId = primary.Id, IsPrimary = true, OwnerId = primary.OwnerId, LinkedAt = now } };
            links.AddRange(extras.Select(process => new RiskProcess { ProcessId = process.Id, IsPrimary = false, OwnerId = process.OwnerId, LinkedAt = now }));
            domain.State.Risks.Insert(0, new RiskItem
            {
                Id = id,
                Title = title,
                Status = "Черновик",
                HazardId = hazard.Id,
                CategoryId = category.Id,
                Description = description,
                AuthorId = Me(domain).Id,
                SourceKey = key,
                Processes = links,
                CreatedAt = now
            });
            AppendLog(domain, id, $"Риск импортирован, внешний ключ {key}");
            created++;
        }

        var saved = SaveState(domain);
        if (!saved.Ok) return Results.BadRequest(new { ok = false, message = saved.Message });
        return Results.Ok(new { ok = true, created, skipped, errors });
    }

    static T? ByName<T>(IEnumerable<T> items, string name) where T : Named =>
        items.FirstOrDefault(item => item.IsActive && string.Equals(item.Name.Trim(), name.Trim(), StringComparison.OrdinalIgnoreCase));
}
