namespace Sur.Api;

static partial class Fn
{
    public static Result ClearExecutor(Domain domain, string treatmentId)
    {
        var treatment = FindTreatment(domain, treatmentId);
        if (treatment?.Status != "Холд") return Fail("Снять исполнителя можно только у меры на холде.");
        if (string.IsNullOrEmpty(treatment.ExecutorId)) return Fail("Исполнитель не назначен.");
        if (!IsTreatmentOwner(domain, treatment) || !HasRight(domain, "assign_treatment_executor"))
            return Fail("Исполнителя снимает владелец процесса меры.");
        var before = domain.State.Users.FirstOrDefault(user => user.Id == treatment.ExecutorId)?.Name ?? treatment.ExecutorId;
        treatment.ExecutorId = null;
        AppendLog(domain, treatment.RiskId, $"С меры «{treatment.Title}» снят исполнитель {before}");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
