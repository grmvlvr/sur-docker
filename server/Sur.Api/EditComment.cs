namespace Sur.Api;

static partial class Fn
{
    public static Result EditComment(Domain domain, string commentId, string text)
    {
        var comment = domain.State.Comments.FirstOrDefault(item => item.Id == commentId && !item.IsDeleted);
        if (comment == null) return Fail("Комментарий не найден.");
        if (comment.IsSystem) return Fail("Системный комментарий изменить нельзя.");
        if (string.IsNullOrWhiteSpace(text)) return Fail("Введите комментарий.");
        if (!HasRight(domain, "view_risk") || !HasRight(domain, "comment_risk")) return Fail("Недостаточно прав для комментария.");
        if (!IsAdmin(domain) && comment.AuthorId != Me(domain).Id) return Fail("Можно изменить только свой комментарий.");
        comment.Text = text.Trim();
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
