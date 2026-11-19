namespace Sur.Api;

static partial class Fn
{
    public static void SystemComment(Domain domain, string riskId, string text) =>
        domain.State.Comments.Insert(0, new CommentItem
        {
            Id = "c" + ++domain.State.Seq,
            RiskId = riskId,
            Text = text,
            AuthorId = Me(domain).Id,
            At = Stamp(),
            IsSystem = true
        });
}
