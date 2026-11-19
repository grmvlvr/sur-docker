namespace Sur.Api;

static partial class Fn
{
    public static Result EditPassport(Domain domain, string id, RiskInput input)
    {
        var risk = FindRisk(domain, id);
        if (risk == null) return Fail("Риск не найден.");
        if (risk.Status != "Черновик") return Fail("Паспорт редактируется только в статусе «Черновик».");
        if (!IsPrimaryOwner(domain, risk) || !HasRight(domain, "edit_risk")) return Fail("Паспорт редактирует владелец основного процесса.");
        if (string.IsNullOrWhiteSpace(input.Title) || string.IsNullOrWhiteSpace(input.Description))
            return Fail("Укажите название и описание риска.");
        if (FindHazard(domain, input.HazardId) is not { IsActive: true }) return Fail("Выберите активную опасность.");
        if (FindCategory(domain, input.CategoryId) is not { IsActive: true }) return Fail("Выберите активную категорию.");
        if (!string.IsNullOrWhiteSpace(input.ObjectId) && domain.State.Objects.FirstOrDefault(item => item.Id == input.ObjectId) is not { IsActive: true, Hourly: >= 0 })
            return Fail("Объект недоступен: нужна активная запись со стоимостью часа простоя.");
        risk.Title = input.Title.Trim();
        risk.HazardId = input.HazardId;
        risk.CategoryId = input.CategoryId;
        risk.Description = input.Description.Trim();
        risk.ObjectId = string.IsNullOrWhiteSpace(input.ObjectId) ? null : input.ObjectId;
        risk.Top = input.Top;
        AppendLog(domain, id, "Изменён паспорт черновика");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
