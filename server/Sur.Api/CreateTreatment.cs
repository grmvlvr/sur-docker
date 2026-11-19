namespace Sur.Api;

static partial class Fn
{
    public static Result CreateTreatment(Domain domain, string riskId, TreatmentInput input)
    {
        var risk = FindRisk(domain, riskId);
        if (risk?.Status != "Активен") return Fail("Мера создаётся для активного риска.");
        if (risk.Assessment?.ResponseMode is not ("treat" or "split"))
            return Fail("Сначала выберите режим «Обработать» или «Разделить».");
        if (!HasRight(domain, "view_risk") || !HasRight(domain, "create_treatment")) return Fail("Недостаточно прав для создания меры.");
        if (risk.Processes.All(item => item.ProcessId != input.ProcessId || !string.IsNullOrEmpty(item.UnlinkedAt))) return Fail("Процесс не связан с риском.");
        if (string.IsNullOrWhiteSpace(input.Title) || string.IsNullOrWhiteSpace(input.Due)) return Fail("Укажите название и срок меры.");
        if (input.CostPlanned < 0) return Fail("Плановая стоимость не может быть отрицательной.");
        var id = "t" + ++domain.State.Seq;
        domain.State.Treatments.Insert(0, new TreatmentItem
        {
            Id = id,
            RiskId = riskId,
            ProcessId = input.ProcessId,
            Title = input.Title.Trim(),
            Description = input.Description.Trim(),
            CostPlanned = input.CostPlanned,
            Due = input.Due,
            Status = "Черновик",
            AuthorId = Me(domain).Id
        });
        AppendLog(domain, riskId, $"Создана мера «{input.Title.Trim()}»");
        var saved = SaveState(domain);
        return saved.Ok ? Ok(id) : saved;
    }
}
