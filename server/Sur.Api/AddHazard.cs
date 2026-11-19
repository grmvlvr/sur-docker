namespace Sur.Api;

static partial class Fn
{
    public static Result AddHazard(Domain domain, string name)
    {
        if (!IsAdmin(domain)) return Fail("Справочник опасностей меняет администратор.");
        if (string.IsNullOrWhiteSpace(name)) return Fail("Укажите название.");
        domain.State.Hazards.Add(new HazardItem { Id = "h" + ++domain.State.Seq, Name = name.Trim(), Group = "Прочее", IsActive = true });
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
