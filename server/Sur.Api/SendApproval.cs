namespace Sur.Api;

static partial class Fn
{
    public static Result SendApproval(Domain domain, string treatmentId)
    {
        var treatment = FindTreatment(domain, treatmentId);
        if (treatment?.Status != "Черновик") return Fail("На согласование уходит мера в статусе «Черновик».");
        if (!IsTreatmentOwner(domain, treatment)) return Fail("Отправляет владелец процесса меры.");
        var level = ApprovalLevel(domain, treatment.CostPlanned);
        if (level == null) return Fail("Согласование не требуется. Передайте меру к выполнению.");
        var directorateId = FindProcess(domain, treatment.ProcessId)!.DirectorateId;
        if (SlotTaken(domain, treatment.RiskId, directorateId, treatment.Id))
            return Fail("Для этой дирекции уже есть мера на согласовании или в работе.");
        treatment.Status = "На согласовании";
        var thresholds = domain.State.Thresholds;
        var bound = level == "CEO" ? thresholds.CeoCost : level == "DIRECTOR" ? thresholds.DirectorCost : thresholds.OwnerCost;
        domain.State.Approvals.Insert(0, new ApprovalItem
        {
            Id = "a" + ++domain.State.Seq,
            TreatmentId = treatmentId,
            Level = level,
            Status = "На согласовании",
            TriggerCondition = $"плановая стоимость {treatment.CostPlanned} достигла порога {level}: от {bound} ₽",
            CreatedAt = Stamp()
        });
        AppendLog(domain, treatment.RiskId, $"Мера «{treatment.Title}» отправлена на согласование уровня {Rules.Levels[level]}");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
