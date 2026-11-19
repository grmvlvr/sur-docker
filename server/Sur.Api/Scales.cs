namespace Sur.Api;

static partial class Fn
{
    public static object Scales(Domain domain) => new
    {
        likelihood = domain.State.Likelihoods.Select(item => new { level = item.Level, name = item.Name }),
        impact = domain.State.Bands.Select(band => new { level = band.Level, from = band.From, to = band.To, name = band.Name }),
        levels = Rules.Levels
    };
}
