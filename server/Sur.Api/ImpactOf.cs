namespace Sur.Api;

static partial class Fn
{
    public static int ImpactOf(Domain domain, decimal loss) =>
        domain.State.Bands.OrderBy(band => band.From).Last(band => loss >= band.From).Level;
}
