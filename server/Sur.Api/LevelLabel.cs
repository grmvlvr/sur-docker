namespace Sur.Api;

static partial class Fn
{
    public static string LevelLabel(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return "не сохранён";
        return Rules.Levels.TryGetValue(code, out var name) ? name : code;
    }
}
