namespace Sur.Api;

static partial class Fn
{
    public static Result ResumeTreatment(Domain domain, string treatmentId)
    {
        var treatment = FindTreatment(domain, treatmentId);
        if (treatment?.Status != "Холд") return Fail("Возобновить можно меру на холде.");
        if (string.IsNullOrEmpty(treatment.ExecutorId)) return Fail("Для возврата в работу нужен исполнитель.");
        if (!IsTreatmentOwner(domain, treatment) || !HasRight(domain, "execute_treatment")) return Fail("Статус меняет владелец процесса меры.");
        var risk = FindRisk(domain, treatment.RiskId);
        if (risk?.Status != "Активен" || risk.Assessment?.ResponseMode is not ("treat" or "split"))
            return Fail("Вернуть меру в работу можно только у активного риска в режиме обработки или разделения.");
        var directorateId = FindProcess(domain, treatment.ProcessId)!.DirectorateId;
        if (SlotTaken(domain, treatment.RiskId, directorateId, treatment.Id))
            return Fail("Для этой дирекции уже есть мера на согласовании или в работе.");
        treatment.Status = "В работе";
        AppendLog(domain, treatment.RiskId, $"Мера «{treatment.Title}» возвращена в работу");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
