namespace Sur.Api;

static partial class Fn
{
    public static Result SaveDirectorate(Domain domain, string? id, NamedInput input)
    {
        if (!IsAdmin(domain)) return Fail("Оргструктуру меняет администратор.");
        if (string.IsNullOrWhiteSpace(input.Name)) return Fail("Укажите название дирекции.");
        if (string.IsNullOrEmpty(id))
            domain.State.Directorates.Add(new Named { Id = "d" + ++domain.State.Seq, Name = input.Name.Trim(), IsActive = true });
        else
        {
            var item = domain.State.Directorates.FirstOrDefault(entry => entry.Id == id);
            if (item == null) return Fail("Дирекция не найдена.");
            item.Name = input.Name.Trim();
            item.IsActive = input.IsActive;
        }
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
