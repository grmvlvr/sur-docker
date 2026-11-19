namespace Sur.Api;

static partial class Fn
{
    public static Result HoldTreatment(Domain domain, string treatmentId)
    {
        var treatment = FindTreatment(domain, treatmentId);
        if (treatment?.Status != "В работе") return Fail("На холд уходит мера в работе.");
        if (!IsTreatmentOwner(domain, treatment) || !HasRight(domain, "execute_treatment")) return Fail("Статус меняет владелец процесса меры.");
        treatment.Status = "Холд";
        AppendLog(domain, treatment.RiskId, $"Мера «{treatment.Title}» поставлена на холд");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
