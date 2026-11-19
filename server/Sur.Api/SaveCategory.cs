namespace Sur.Api;

static partial class Fn
{
    public static Result SaveCategory(Domain domain, string? id, NamedInput input)
    {
        if (!IsAdmin(domain)) return Fail("Справочники меняет администратор.");
        if (string.IsNullOrWhiteSpace(input.Name)) return Fail("Укажите название категории.");
        if (domain.State.Categories.Any(item => item.IsActive && item.Name == input.Name.Trim() && item.Id != id))
            return Fail("Активная категория с таким названием уже есть.");
        if (string.IsNullOrEmpty(id))
            domain.State.Categories.Add(new Named { Id = "c" + ++domain.State.Seq, Name = input.Name.Trim(), IsActive = true });
        else
        {
            var item = domain.State.Categories.FirstOrDefault(entry => entry.Id == id);
            if (item == null) return Fail("Категория не найдена.");
            item.Name = input.Name.Trim();
            item.IsActive = input.IsActive;
        }
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
