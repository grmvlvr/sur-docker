namespace Sur.Api;

static partial class Fn
{
    public static Result ReopenRisk(Domain domain, string id)
    {
        var risk = FindRisk(domain, id);
        if (risk == null || risk.Status is not ("Принят" or "Закрыт"))
            return Fail("Вернуть на переоценку можно принятый или закрытый риск.");
        if (!IsPrimaryOwner(domain, risk) || !HasRight(domain, "edit_risk")) return Fail("Возвращает владелец основного процесса.");
        var wasClosed = risk.Status == "Закрыт";
        if (risk.Assessment != null)
        {
            risk.Assessment.IsActive = false;
            risk.History.Insert(0, risk.Assessment);
            risk.Assessment = null;
        }
        risk.AcceptanceLevel = null;
        risk.CloseReason = null;
        risk.Status = "Ожидает переоценки";
        if (wasClosed) SystemComment(domain, id, $"{Me(domain).Name} вернул закрытый риск на переоценку.");
        AppendLog(domain, id, "Риск возвращён на переоценку");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
