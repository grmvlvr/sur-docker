namespace Sur.Api;

static partial class Fn
{
    public static object ListApprovals(Domain domain)
    {
        bool Mine(ApprovalItem item)
        {
            if (IsAdmin(domain) && item.Status == "Согласовано")
            {
                var approved = FindTreatment(domain, item.TreatmentId);
                return approved?.Status == "Согласовано";
            }
            if (item.Status != "На согласовании") return false;
            var treatment = FindTreatment(domain, item.TreatmentId);
            if (treatment == null) return false;
            var risk = FindRisk(domain, treatment.RiskId);
            if (risk == null) return false;
            var directorateId = FindProcess(domain, treatment.ProcessId)?.DirectorateId;
            var owner = risk.Processes.Any(link => link.ProcessId == treatment.ProcessId && link.OwnerId == Me(domain).Id);
            var me = Me(domain);
            return IsAdmin(domain)
                || (item.Level == "OWNER" && owner && HasRight(domain, "approve_owner_treatment"))
                || (item.Level == "DIRECTOR" && HasRight(domain, "approve_director_treatment") && me.DirectorateId == directorateId)
                || (item.Level == "CEO" && HasRight(domain, "approve_ceo_treatment") && (me.Roles.Contains("CEO") || me.DirectorateId == directorateId));
        }

        return domain.State.Approvals.Where(Mine).Select(item =>
        {
            var treatment = FindTreatment(domain, item.TreatmentId);
            return new { item.Id, item.TreatmentId, item.Level, item.Status, title = treatment?.Title, riskId = treatment?.RiskId, cost = treatment?.CostPlanned };
        });
    }
}
