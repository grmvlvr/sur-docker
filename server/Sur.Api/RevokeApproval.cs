namespace Sur.Api;

static partial class Fn
{
    public static Result RevokeApproval(Domain domain, string approvalId)
    {
        if (!IsAdmin(domain)) return Fail("Отозвать запрос может только администратор.");
        var request = domain.State.Approvals.FirstOrDefault(item => item.Id == approvalId);
        var treatment = request == null ? null : FindTreatment(domain, request.TreatmentId);
        if (request == null || treatment == null) return Fail("Запрос согласования не найден.");
        if (request.Status == "На согласовании")
        {
            request.Status = "Отозван";
            treatment.Status = "Черновик";
            AppendLog(domain, treatment.RiskId, $"Администратор отозвал согласование меры «{treatment.Title}»");
        }
        else if (request.Status == "Согласовано" && treatment.Status == "Согласовано")
        {
            request.Status = "Отозван";
            treatment.Status = "Отменено";
            AppendLog(domain, treatment.RiskId, $"Администратор отозвал согласованную меру «{treatment.Title}»");
            var risk = FindRisk(domain, treatment.RiskId);
            if (risk != null) FinishCycle(domain, risk);
        }
        else return Fail("Этот запрос отозвать нельзя.");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
