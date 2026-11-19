namespace Sur.Api;

static partial class Fn
{
    public static bool SameDirectorate(Domain domain, ProcessItem process) => IsAdmin(domain) || process.DirectorateId == Me(domain).DirectorateId;
}
