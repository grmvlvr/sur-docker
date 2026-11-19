namespace Sur.Api;

static partial class Fn
{
    public static Result CloseRisk(Domain domain, string id, string reason)
    {
        var risk = FindRisk(domain, id);
        if (risk == null) return Fail("Риск не найден.");
        if (risk.Status is not ("Черновик" or "Активен" or "Ожидает переоценки" or "Принят"))
            return Fail("Риск в этом статусе закрыть нельзя.");
        if (!IsPrimaryOwner(domain, risk) || !HasRight(domain, "close_risk")) return Fail("Закрывает владелец основного процесса.");
        if (string.IsNullOrWhiteSpace(reason)) return Fail("Укажите причину закрытия.");
        if (TreatmentsOf(domain, id).Any(item => Rules.OpenTreatment.Contains(item.Status)))
            return Fail("Есть незавершённые меры.");
        risk.Status = "Закрыт";
        risk.CloseReason = reason.Trim();
        SystemComment(domain, id, $"Риск закрыт. Причина: {reason.Trim()}. Инициатор: {Me(domain).Name}.");
        AppendLog(domain, id, "Риск закрыт");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
