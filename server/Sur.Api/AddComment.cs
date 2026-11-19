namespace Sur.Api;

static partial class Fn
{
    public static Result AddComment(Domain domain, string riskId, string text)
    {
        if (FindRisk(domain, riskId) == null) return Fail("Риск не найден.");
        if (string.IsNullOrWhiteSpace(text)) return Fail("Введите комментарий.");
        domain.State.Comments.Insert(0, new CommentItem
        {
            Id = "c" + ++domain.State.Seq,
            RiskId = riskId,
            Text = text.Trim(),
            AuthorId = Me(domain).Id,
            At = Stamp()
        });
        AppendLog(domain, riskId, "Добавлен комментарий");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
