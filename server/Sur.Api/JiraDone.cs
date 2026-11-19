namespace Sur.Api;

static partial class Fn
{
    public static Result JiraDone(Domain domain, string treatmentId, decimal costActual)
    {
        var treatment = FindTreatment(domain, treatmentId);
        if (string.IsNullOrEmpty(treatment?.Jira)) return Fail("Внешняя задача ещё не создана.");
        if (treatment.Status != "В работе") return Fail("Завершить можно только меру в работе.");
        if (costActual < 0) return Fail("Укажите фактическую стоимость.");
        treatment.CostActual = costActual;
        treatment.Status = "Готово";
        treatment.TaskProvider ??= "MOCK";
        treatment.TaskStatus = "Готово";
        treatment.TaskError = null;
        var mode = treatment.TaskProvider == "JIRA" ? "Jira" : "MOCK";
        AppendLog(domain, treatment.RiskId, $"Внешняя задача {treatment.Jira} ({mode}) закрыта, мера «{treatment.Title}» выполнена");
        FinishCycle(domain, FindRisk(domain, treatment.RiskId)!);
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
