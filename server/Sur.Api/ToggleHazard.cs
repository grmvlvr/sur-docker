namespace Sur.Api;

static partial class Fn
{
    public static Result ToggleHazard(Domain domain, string id, bool isActive)
    {
        if (!IsAdmin(domain)) return Fail("Справочник опасностей меняет администратор.");
        var hazard = FindHazard(domain, id);
        if (hazard == null) return Fail("Опасность не найдена.");
        hazard.IsActive = isActive;
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
