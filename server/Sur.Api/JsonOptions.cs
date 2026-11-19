using System.Text.Json;

namespace Sur.Api;

static partial class Fn
{
    public static JsonSerializerOptions JsonOptions() => new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
}
