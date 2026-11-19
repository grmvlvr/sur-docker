namespace Sur.Api;

static partial class Fn
{
    public static Named? FindCategory(Domain domain, string? id) => domain.State.Categories.FirstOrDefault(item => item.Id == id);
}
