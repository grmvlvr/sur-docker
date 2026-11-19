namespace Sur.Api;

static partial class Fn
{
    public static ProcessItem? FindProcess(Domain domain, string id) => domain.State.Processes.FirstOrDefault(p => p.Id == id);
}
