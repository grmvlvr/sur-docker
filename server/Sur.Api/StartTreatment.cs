namespace Sur.Api;

static partial class Fn
{
    public static Result StartTreatment(Domain domain, string treatmentId, string executorId, bool failMock = false)
    {
        var treatment = FindTreatment(domain, treatmentId);
        var direct = treatment != null && treatment.Status == "Черновик" && ApprovalLevel(domain, treatment.CostPlanned) == null;
        if (treatment?.Status != "Согласовано" && !direct) return Fail("К выполнению уходит согласованная мера или черновик без согласования.");
        if (!IsTreatmentOwner(domain, treatment!) || !HasRight(domain, "assign_treatment_executor")) return Fail("Исполнителя назначает владелец процесса меры.");
        var directorateId = FindProcess(domain, treatment.ProcessId)!.DirectorateId;
        if (SlotTaken(domain, treatment.RiskId, directorateId, treatment.Id))
            return Fail("Для этой дирекции уже есть мера на согласовании или в работе.");
        var executor = domain.State.Users.FirstOrDefault(u => u.Id == executorId);
        var dir = FindProcess(domain, treatment.ProcessId)!.DirectorateId;
        if (executor == null || executor.DirectorateId != dir)
            return Fail("Исполнитель должен быть сотрудником дирекции процесса.");
        if (!executor.Roles.Contains("OWNER") && !executor.Roles.Contains("EMPLOYEE"))
            return Fail("Исполнителем может быть владелец или линейный сотрудник.");
        treatment.ExecutorId = executorId;
        treatment.Status = "Ожидает выполнения";
        AttachMock(domain, treatment, failMock);
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
