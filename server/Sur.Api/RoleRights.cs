namespace Sur.Api;

static class RoleRights
{
    public static readonly Dictionary<string, string[]> Map = new()
    {
        ["EMPLOYEE"] = ["view_risk", "comment_risk", "comment_treatment", "create_risk", "create_treatment", "execute_treatment"],
        ["OWNER"] = ["view_risk", "comment_risk", "comment_treatment", "create_risk", "edit_risk", "close_risk", "assess_risk", "accept_owner_risk", "create_treatment", "edit_treatment", "assign_treatment_executor", "execute_treatment", "approve_owner_treatment", "split_risk", "transfer_risk", "view_report", "export_report"],
        ["DIRECTOR"] = ["view_risk", "comment_risk", "comment_treatment", "accept_director_risk", "approve_director_treatment", "view_report", "export_report"],
        ["CEO"] = ["view_risk", "comment_risk", "comment_treatment", "accept_ceo_risk", "approve_ceo_treatment", "view_report", "export_report"],
        ["RISK_SPECIALIST"] = ["view_risk", "comment_risk", "comment_treatment", "accept_ceo_risk", "approve_ceo_treatment", "view_report", "export_report"],
        ["ADMIN"] = ["admin"]
    };
}
