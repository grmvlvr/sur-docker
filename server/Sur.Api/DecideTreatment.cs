namespace Sur.Api;

static partial class Fn
{
    public static Result DecideTreatment(Domain domain, string treatmentId, bool approve, string? comment)
    {
        var treatment = FindTreatment(domain, treatmentId);
        var request = domain.State.Approvals.FirstOrDefault(a => a.TreatmentId == treatmentId && a.Status == "На согласовании");
        if (treatment == null || request == null) return Fail("Нет запроса на согласование.");
        var risk = FindRisk(domain, treatment.RiskId)!;
        var dir = FindProcess(domain, treatment.ProcessId)!.DirectorateId;
        var owner = risk.Processes.Any(p => p.ProcessId == treatment.ProcessId && p.OwnerId == Me(domain).Id);
        var me = Me(domain);
        var allowed = IsAdmin(domain)
            || (request.Level == "OWNER" && owner && HasRight(domain, "approve_owner_treatment"))
            || (request.Level == "DIRECTOR" && HasRight(domain, "approve_director_treatment") && me.DirectorateId == dir)
            || (request.Level == "CEO" && HasRight(domain, "approve_ceo_treatment") && (me.Roles.Contains("CEO") || me.DirectorateId == dir));
        if (!allowed) return Fail("Согласование этого уровня вам недоступно.");
        var note = (comment ?? "").Trim();
        if (!approve && note.Length == 0) return Fail("Укажите причину отклонения.");
        if (approve && SlotTaken(domain, treatment.RiskId, dir, treatment.Id))
            return Fail("Для этой дирекции уже есть мера на согласовании или в работе.");
        request.Status = approve ? "Согласовано" : "Отклонено";
        request.DecidedAt = Stamp();
        request.DecidedById = Me(domain).Id;
        request.DecisionComment = note.Length == 0 ? null : note;
        treatment.Status = approve ? "Согласовано" : "Черновик";
        var detail = note.Length == 0 ? "" : $": {note}";
        AppendLog(domain, treatment.RiskId, approve
            ? $"Мера «{treatment.Title}» согласована{detail}"
            : $"Мера «{treatment.Title}» отклонена{detail}");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
