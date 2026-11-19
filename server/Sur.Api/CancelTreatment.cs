namespace Sur.Api;

static partial class Fn
{
    public static Result CancelTreatment(Domain domain, string treatmentId)
    {
        var treatment = FindTreatment(domain, treatmentId);
        if (treatment == null) return Fail("Мера не найдена.");
        if (treatment.Status is "Готово" or "Отменено") return Fail("Мера уже завершена.");
        var ownerCancelsDraft = treatment.Status == "Черновик" && IsTreatmentOwner(domain, treatment);
        if (!IsAdmin(domain) && !ownerCancelsDraft) return Fail("Черновик отменяет владелец процесса, остальные меры — администратор.");
        var wasOpen = Rules.OpenTreatment.Contains(treatment.Status);
        treatment.Status = "Отменено";
        AppendLog(domain, treatment.RiskId, $"Отменена мера «{treatment.Title}»");
        if (wasOpen) FinishCycle(domain, FindRisk(domain, treatment.RiskId)!);
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
