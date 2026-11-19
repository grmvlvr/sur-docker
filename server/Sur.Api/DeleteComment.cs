namespace Sur.Api;

static partial class Fn
{
    public static Result DeleteComment(Domain domain, string commentId)
    {
        var comment = domain.State.Comments.FirstOrDefault(item => item.Id == commentId && !item.IsDeleted);
        if (comment == null) return Fail("Комментарий не найден.");
        if (comment.IsSystem) return Fail("Системный комментарий удалить нельзя.");
        if (!HasRight(domain, "view_risk") || !HasRight(domain, "comment_risk")) return Fail("Недостаточно прав для комментария.");
        if (!IsAdmin(domain) && comment.AuthorId != Me(domain).Id) return Fail("Можно удалить только свой комментарий.");
        comment.IsDeleted = true;
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
