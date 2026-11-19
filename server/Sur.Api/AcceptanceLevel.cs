namespace Sur.Api;

static partial class Fn
{
    public static string AcceptanceLevel(Domain domain, int score)
    {
        var thresholds = domain.State.Thresholds;
        if (score <= thresholds.OwnerMaxScore) return "OWNER";
        if (score <= thresholds.DirectorMaxScore) return "DIRECTOR";
        if (score <= thresholds.CeoMaxScore) return "CEO";
        return "TREATMENT_REQUIRED";
    }
}
