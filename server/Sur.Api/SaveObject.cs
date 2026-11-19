namespace Sur.Api;

static partial class Fn
{
    public static Result SaveObject(Domain domain, string? id, ObjectInput input)
    {
        if (!IsAdmin(domain)) return Fail("Объекты меняет администратор.");
        if (string.IsNullOrWhiteSpace(input.Name) || input.Hourly < 0) return Fail("Укажите название и неотрицательную стоимость часа.");
        if (string.IsNullOrEmpty(id))
            domain.State.Objects.Add(new AssetItem { Id = "o" + ++domain.State.Seq, Name = input.Name.Trim(), Hourly = input.Hourly, IsActive = true });
        else
        {
            var asset = domain.State.Objects.FirstOrDefault(item => item.Id == id);
            if (asset == null) return Fail("Объект не найден.");
            var hourlyChanged = asset.Hourly != input.Hourly;
            asset.Name = input.Name.Trim();
            asset.Hourly = input.Hourly;
            asset.IsActive = input.IsActive;
            if (hourlyChanged) RecalcHourly(domain, asset.Id, asset.Hourly);
        }
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
