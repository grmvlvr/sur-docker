namespace Sur.Api;

static partial class Fn
{
    public static Assessment CopyAssessment(Assessment current, int impact, int score, string level, decimal loss, decimal? hourly) => new()
    {
        Likelihood = current.Likelihood,
        LossAmount = loss,
        DowntimeWeeks = current.DowntimeWeeks,
        DowntimeDays = current.DowntimeDays,
        DowntimeHours = current.DowntimeHours,
        DowntimeTotalHours = current.DowntimeTotalHours,
        HourlyDowntimeCost = hourly,
        Impact = impact,
        Score = score,
        Level = level,
        ResponseMode = current.ResponseMode,
        IsActive = true,
        At = current.At,
        AuthorId = current.AuthorId
    };
}
