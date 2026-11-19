using System.Text.Json;

namespace Sur.Api;

public sealed class Domain
{
    public string StorePath { get; }
    public object Gate { get; } = new();
    public AppState State { get; }

    public Domain(IWebHostEnvironment env)
    {
        StorePath = Path.Combine(env.ContentRootPath, "App_Data", "state.json");
        Directory.CreateDirectory(Path.GetDirectoryName(StorePath)!);
        if (!File.Exists(StorePath))
        {
            State = Fn.SeedState();
            Fn.EnsureDefaults(State);
            return;
        }

        try
        {
            State = JsonSerializer.Deserialize<AppState>(File.ReadAllText(StorePath), Fn.JsonOptions()) ?? Fn.SeedState();
            Fn.EnsureDefaults(State);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("Файл состояния повреждён: App_Data/state.json");
        }
    }
}
