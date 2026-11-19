namespace Sur.Api;

static partial class Fn
{
    public static void AttachMock(Domain domain, TreatmentItem treatment, bool fail)
    {
        treatment.TaskProvider = "MOCK";
        if (!string.IsNullOrEmpty(treatment.Jira)) return;
        if (fail)
        {
            treatment.Jira = null;
            treatment.TaskUrl = null;
            treatment.TaskStatus = null;
            treatment.TaskCreatedAt = null;
            treatment.TaskError = "Внешняя задача не создана (режим MOCK). Мера, согласование и назначение сохранены.";
            AppendLog(domain, treatment.RiskId, $"Внешняя задача для меры «{treatment.Title}» не создана (режим MOCK)");
            return;
        }

        var key = "SUR-" + (30 + ++domain.State.Seq);
        treatment.Jira = key;
        treatment.TaskUrl = null;
        treatment.TaskStatus = "Создана";
        treatment.TaskError = null;
        treatment.TaskCreatedAt = Stamp();
        AppendLog(domain, treatment.RiskId, $"Мера «{treatment.Title}» связана с внешней задачей {key} (режим MOCK)");
    }
}
