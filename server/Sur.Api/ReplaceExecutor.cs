namespace Sur.Api;

static partial class Fn
{
    public static Result ReplaceExecutor(Domain domain, string treatmentId, string executorId)
    {
        var treatment = FindTreatment(domain, treatmentId);
        if (treatment == null) return Fail("Мера не найдена.");
        if (treatment.Status is not ("Ожидает выполнения" or "В работе" or "Холд"))
            return Fail("Исполнителя можно заменить у меры в ожидании, в работе или на холде.");
        if (string.IsNullOrEmpty(treatment.ExecutorId) && treatment.Status != "Холд") return Fail("Сначала назначьте исполнителя.");
        if (!IsTreatmentOwner(domain, treatment) || !HasRight(domain, "assign_treatment_executor"))
            return Fail("Исполнителя меняет владелец процесса меры.");
        var directorateId = FindProcess(domain, treatment.ProcessId)!.DirectorateId;
        var valid = ValidExecutor(domain, executorId, directorateId);
        if (!valid.Ok) return valid;
        var before = treatment.ExecutorId == null
            ? "не назначен"
            : domain.State.Users.FirstOrDefault(user => user.Id == treatment.ExecutorId)?.Name ?? treatment.ExecutorId;
        var after = domain.State.Users.FirstOrDefault(user => user.Id == executorId)?.Name ?? executorId;
        treatment.ExecutorId = executorId;
        AppendLog(domain, treatment.RiskId, $"У меры «{treatment.Title}» исполнитель изменён: {before} → {after}");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
