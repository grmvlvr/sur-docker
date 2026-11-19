namespace Sur.Api;

static partial class Fn
{
    public static Result CreateRisk(Domain domain, RiskInput input)
    {
        var process = FindProcess(domain, input.ProcessId);
        if (process is not { IsActive: true }) return Fail("Выберите активный процесс.");
        if (!HasRight(domain, "create_risk")) return Fail("Недостаточно прав для создания риска.");
        if (!IsAdmin(domain) && !SameDirectorate(domain, process)) return Fail("Сотрудник создаёт риск только по процессу своей дирекции.");
        if (string.IsNullOrWhiteSpace(input.Title) || string.IsNullOrWhiteSpace(input.Description))
            return Fail("Укажите название и описание риска.");
        if (FindHazard(domain, input.HazardId) is not { IsActive: true }) return Fail("Выберите активную опасность.");
        if (FindCategory(domain, input.CategoryId) is not { IsActive: true }) return Fail("Выберите активную категорию.");
        if (!string.IsNullOrWhiteSpace(input.ObjectId) && domain.State.Objects.FirstOrDefault(item => item.Id == input.ObjectId) is not { IsActive: true, Hourly: >= 0 })
            return Fail("Объект недоступен: нужна активная запись со стоимостью часа простоя.");
        var extras = input.AdditionalProcessIds
            .Where(id => !string.IsNullOrWhiteSpace(id) && id != process.Id)
            .Distinct()
            .ToList();
        foreach (var pid in extras)
        {
            var extra = FindProcess(domain, pid);
            if (extra is not { IsActive: true }) return Fail("Дополнительный процесс недоступен.");
            if (!IsAdmin(domain) && !SameDirectorate(domain, extra)) return Fail("Дополнительный процесс должен быть из вашей дирекции.");
        }

        var id = (246000 + ++domain.State.Seq).ToString();
        var now = Stamp();
        var links = new List<RiskProcess>
        {
            new() { ProcessId = process.Id, IsPrimary = true, OwnerId = process.OwnerId, LinkedAt = now }
        };
        links.AddRange(extras.Select(pid =>
        {
            var item = FindProcess(domain, pid)!;
            return new RiskProcess { ProcessId = item.Id, IsPrimary = false, OwnerId = item.OwnerId, LinkedAt = now };
        }));
        domain.State.Risks.Insert(0, new RiskItem
        {
            Id = id,
            Title = input.Title.Trim(),
            Status = "Черновик",
            HazardId = input.HazardId,
            CategoryId = input.CategoryId,
            Description = input.Description.Trim(),
            AuthorId = Me(domain).Id,
            Processes = links,
            ObjectId = string.IsNullOrWhiteSpace(input.ObjectId) ? null : input.ObjectId,
            CreatedAt = now,
            Top = input.Top
        });
        AppendLog(domain, id, links.Count > 1
            ? $"Создан черновик риска с {links.Count} процессными связями"
            : "Создан черновик риска");
        var saved = SaveState(domain);
        return saved.Ok ? Ok(id) : saved;
    }
}
