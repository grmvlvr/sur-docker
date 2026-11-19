namespace Sur.Api;

static partial class Fn
{
    public static Result BeginWork(Domain domain, string treatmentId)
    {
        var treatment = FindTreatment(domain, treatmentId);
        if (treatment?.Status != "Ожидает выполнения") return Fail("В работу переводится мера, ожидающая выполнения.");
        if (string.IsNullOrEmpty(treatment.ExecutorId)) return Fail("Сначала назначьте исполнителя.");
        if (!IsTreatmentOwner(domain, treatment) || !HasRight(domain, "execute_treatment"))
            return Fail("Статус меняет владелец процесса меры.");
        treatment.Status = "В работе";
        AppendLog(domain, treatment.RiskId, $"Мера «{treatment.Title}» взята в работу");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
