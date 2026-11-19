namespace Sur.Api;

static partial class Fn
{
    public static Result AcceptRisk(Domain domain, string id)
    {
        var risk = FindRisk(domain, id);
        if (risk?.Status != "Активен" || risk.Assessment is not { ResponseMode: null })
            return Fail("Принять можно активный риск без выбранного режима.");
        if (TreatmentsOf(domain, id).Any(t => Rules.OpenTreatment.Contains(t.Status))) return Fail("Есть незавершённые меры.");
        var level = risk.Assessment.Level;
        var dir = PrimaryProcess(domain, risk).DirectorateId;
        var me = Me(domain);
        var allowed = IsAdmin(domain)
            || (level == "OWNER" && IsPrimaryOwner(domain, risk) && HasRight(domain, "accept_owner_risk"))
            || (level == "DIRECTOR" && HasRight(domain, "accept_director_risk") && me.DirectorateId == dir)
            || (level == "CEO" && HasRight(domain, "accept_ceo_risk") && (me.Roles.Contains("CEO") || me.DirectorateId == dir));
        if (level == "TREATMENT_REQUIRED" && !IsAdmin(domain))
            return Fail("Риск этого уровня нельзя принять. Его нужно обрабатывать.");
        if (!allowed) return Fail($"Принятие уровня «{Rules.Levels[level]}» вам недоступно.");
        risk.Assessment.ResponseMode = "accept";
        risk.AcceptanceLevel = level;
        risk.Status = "Принят";
        AppendLog(domain, id, $"{Me(domain).Name} принял риск уровня {Rules.Levels[level]}");
        var saved = SaveState(domain);
        return saved.Ok ? Ok() : saved;
    }
}
