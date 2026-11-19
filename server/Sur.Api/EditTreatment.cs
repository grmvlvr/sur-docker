namespace Sur.Api;

static partial class Fn
{
    public static Result EditTreatment(Domain domain, string treatmentId, TreatmentInput input)
    {
        var treatment = FindTreatment(domain, treatmentId);
        if (treatment?.Status != "Черновик") return Fail("Мера редактируется только в статусе «Черновик».");
        if (!IsTreatmentOwner(domain, treatment) || !HasRight(domain, "edit_treatment")) return Fail("Черновик редактирует владелец процесса меры.");
        var risk = FindRisk(domain, treatment.RiskId);
        if (risk == null || risk.Processes.All(item => item.ProcessId != input.ProcessId || !string.IsNullOrEmpty(item.UnlinkedAt)))
            return Fail("Процесс не связан с риском.");
        if (string.IsNullOrWhiteSpace(input.Title) || string.IsNullOrWhiteSpace(input.Due)) return Fail("Укажите название и срок меры.");
        if (input.CostPlanned < 0) return Fail("Плановая стоимость не может быть отрицательной.");
        treatment.Title = input.Title.Trim();
        treatment.Description = input.Description.Trim();
        treatment.Due = input.Due;
        treatment.CostPlanned = input.CostPlanned;
        treatment.ProcessId = input.ProcessId;
        AppendLog(domain, treatment.RiskId, $"Изменён черновик меры «{treatment.Title}»");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
