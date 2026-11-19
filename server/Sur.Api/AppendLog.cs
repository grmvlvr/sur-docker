namespace Sur.Api;

static partial class Fn
{
    public static void AppendLog(Domain domain, string riskId, string text) =>
        domain.State.Log.Insert(0, new LogItem
        {
            Id = "l" + ++domain.State.Seq,
            RiskId = riskId,
            Text = text,
            At = Stamp(),
            Author = Me(domain).Name
        });
}
