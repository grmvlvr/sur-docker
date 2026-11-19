namespace Sur.Api;

static partial class Fn
{
    public static Result RetryMock(Domain domain, string treatmentId, bool failMock)
    {
        var treatment = FindTreatment(domain, treatmentId);
        if (treatment == null) return Fail("Мера не найдена.");
        if (!IsTreatmentOwner(domain, treatment)) return Fail("Повторяет владелец процесса меры.");
        if (treatment.Status is not ("Ожидает выполнения" or "В работе"))
            return Fail("Задачу можно создать после передачи меры к выполнению.");
        if (!string.IsNullOrEmpty(treatment.Jira)) return Ok();
        AttachMock(domain, treatment, failMock);
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
