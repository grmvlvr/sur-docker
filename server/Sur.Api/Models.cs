namespace Sur.Api;

public sealed class AppState
{
    public string CurrentUserId { get; set; } = "u-own";
    public bool LoggedIn { get; set; }
    public int Seq { get; set; } = 100;
    public Thresholds Thresholds { get; set; } = new();
    public List<Named> Directorates { get; set; } = [];
    public List<ProcessItem> Processes { get; set; } = [];
    public List<AssetItem> Objects { get; set; } = [];
    public List<HazardItem> Hazards { get; set; } = [];
    public List<Named> Categories { get; set; } = [];
    public List<UserItem> Users { get; set; } = [];
    public List<RoleItem> Roles { get; set; } = [];
    public List<ImpactBand> Bands { get; set; } = [];
    public List<LevelName> Likelihoods { get; set; } = [];
    public List<RiskItem> Risks { get; set; } = [];
    public List<TreatmentItem> Treatments { get; set; } = [];
    public List<ApprovalItem> Approvals { get; set; } = [];
    public List<CommentItem> Comments { get; set; } = [];
    public List<LogItem> Log { get; set; } = [];
    public List<SnapshotItem> Snapshots { get; set; } = [];
}

public sealed class Thresholds
{
    public decimal OwnerCost { get; set; }
    public decimal DirectorCost { get; set; } = 50000;
    public decimal CeoCost { get; set; } = 100000;
    public int OwnerMaxScore { get; set; } = 10;
    public int DirectorMaxScore { get; set; } = 15;
    public int CeoMaxScore { get; set; } = 19;
}

public class Named
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public sealed class ProcessItem : Named
{
    public string DirectorateId { get; set; } = "";
    public string OwnerId { get; set; } = "";
}

public sealed class AssetItem : Named
{
    public decimal Hourly { get; set; }
}

public sealed class HazardItem : Named
{
    public string Group { get; set; } = "";
}

public sealed class UserItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Login { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public List<string> Roles { get; set; } = [];
    public string? DirectorateId { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class RiskProcess
{
    public string ProcessId { get; set; } = "";
    public bool IsPrimary { get; set; }
    public string OwnerId { get; set; } = "";
    public string? LinkedAt { get; set; }
    public string? UnlinkedAt { get; set; }
}

public sealed class Assessment
{
    public int Likelihood { get; set; }
    public decimal LossAmount { get; set; }
    public int? DowntimeWeeks { get; set; }
    public int? DowntimeDays { get; set; }
    public decimal? DowntimeHours { get; set; }
    public decimal? DowntimeTotalHours { get; set; }
    public decimal? HourlyDowntimeCost { get; set; }
    public int Impact { get; set; }
    public int Score { get; set; }
    public string Level { get; set; } = "";
    public string? ResponseMode { get; set; }
    public bool IsActive { get; set; }
    public string? At { get; set; }
    public string? AuthorId { get; set; }
}

public sealed class RiskItem
{
    public string Id { get; set; } = "";
    public string Iso { get; set; } = "9001";
    public string Title { get; set; } = "";
    public string Status { get; set; } = "Черновик";
    public string HazardId { get; set; } = "";
    public string? CategoryId { get; set; }
    public string Description { get; set; } = "";
    public List<RiskProcess> Processes { get; set; } = [];
    public string? ObjectId { get; set; }
    public string CreatedAt { get; set; } = "";
    public string AuthorId { get; set; } = "";
    public string? SourceKey { get; set; }
    public string? AcceptanceLevel { get; set; }
    public string? CloseReason { get; set; }
    public bool Top { get; set; }
    public Assessment? Assessment { get; set; }
    public List<Assessment> History { get; set; } = [];
}

public sealed class TreatmentItem
{
    public string Id { get; set; } = "";
    public string RiskId { get; set; } = "";
    public string ProcessId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal CostPlanned { get; set; }
    public string? Due { get; set; }
    public string Status { get; set; } = "Черновик";
    public string? ExecutorId { get; set; }
    public string? Jira { get; set; }
    public string? TaskProvider { get; set; }
    public string? TaskUrl { get; set; }
    public string? TaskStatus { get; set; }
    public string? TaskError { get; set; }
    public string? TaskCreatedAt { get; set; }
    public string AuthorId { get; set; } = "";
    public decimal? CostActual { get; set; }
}

public sealed class ApprovalItem
{
    public string Id { get; set; } = "";
    public string TreatmentId { get; set; } = "";
    public string Level { get; set; } = "";
    public string Status { get; set; } = "";
    public string? TriggerCondition { get; set; }
    public string? CreatedAt { get; set; }
    public string? DecidedAt { get; set; }
    public string? DecidedById { get; set; }
    public string? DecisionComment { get; set; }
}

public sealed class SnapshotItem
{
    public string Id { get; set; } = "";
    public string RiskId { get; set; } = "";
    public string GeneratedAt { get; set; } = "";
    public string AuthorId { get; set; } = "";
    public string Status { get; set; } = "";
    public string? Error { get; set; }
}

public sealed class CommentItem
{
    public string Id { get; set; } = "";
    public string RiskId { get; set; } = "";
    public string Text { get; set; } = "";
    public string AuthorId { get; set; } = "";
    public string At { get; set; } = "";
    public bool IsSystem { get; set; }
    public bool IsDeleted { get; set; }
}

public sealed class LogItem
{
    public string Id { get; set; } = "";
    public string RiskId { get; set; } = "";
    public string Text { get; set; } = "";
    public string At { get; set; } = "";
    public string Author { get; set; } = "";
}

public sealed class RiskInput
{
    public string Title { get; set; } = "";
    public string HazardId { get; set; } = "";
    public string ProcessId { get; set; } = "";
    public List<string> AdditionalProcessIds { get; set; } = [];
    public string? CategoryId { get; set; }
    public string? ObjectId { get; set; }
    public string Description { get; set; } = "";
    public bool Top { get; set; }
}

public sealed class AssessInput
{
    public int Likelihood { get; set; }
    public decimal? LossAmount { get; set; }
    public int? DowntimeWeeks { get; set; }
    public int? DowntimeDays { get; set; }
    public decimal? DowntimeHours { get; set; }
}

public sealed class ModeInput
{
    public string Mode { get; set; } = "";
    public string? ProcessId { get; set; }
    public List<string> ProcessIds { get; set; } = [];
    public string? PrimaryId { get; set; }
}

public sealed class TreatmentInput
{
    public string Title { get; set; } = "";
    public string ProcessId { get; set; } = "";
    public decimal CostPlanned { get; set; }
    public string? Due { get; set; }
    public string Description { get; set; } = "";
}

public sealed class StartInput
{
    public string ExecutorId { get; set; } = "";
    public bool FailMock { get; set; }
}

public sealed class LoginInput
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class ImportInput
{
    public string Csv { get; set; } = "";
}

public sealed class JiraInput
{
    public decimal CostActual { get; set; }
}

public sealed class CommentInput
{
    public string Text { get; set; } = "";
}

public sealed class HazardInput
{
    public string Name { get; set; } = "";
}

public sealed class CloseInput
{
    public string Reason { get; set; } = "";
}

public sealed class RoleItem
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public List<string> Rights { get; set; } = [];
}

public sealed class ImpactBand
{
    public int Level { get; set; }
    public decimal From { get; set; }
    public decimal? To { get; set; }
    public string Name { get; set; } = "";
}

public sealed class LevelName
{
    public int Level { get; set; }
    public string Name { get; set; } = "";
}

public sealed class RoleInput
{
    public List<string> Rights { get; set; } = [];
    public bool IsActive { get; set; } = true;
}

public sealed class ScaleInput
{
    public List<LevelName> Likelihood { get; set; } = [];
    public List<decimal> ImpactFrom { get; set; } = [];
    public int OwnerMaxScore { get; set; }
    public int DirectorMaxScore { get; set; }
    public int CeoMaxScore { get; set; }
    public bool ApplyToActive { get; set; }
}

public sealed class ThresholdInput
{
    public decimal OwnerCost { get; set; }
    public decimal DirectorCost { get; set; }
    public decimal CeoCost { get; set; }
}

public sealed class NamedInput
{
    public string Name { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public sealed class ProcessInput
{
    public string Name { get; set; } = "";
    public string DirectorateId { get; set; } = "";
    public string OwnerId { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public sealed class ObjectInput
{
    public string Name { get; set; } = "";
    public decimal Hourly { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UserInput
{
    public string Name { get; set; } = "";
    public string Login { get; set; } = "";
    public List<string> Roles { get; set; } = [];
    public string? DirectorateId { get; set; }
    public bool IsActive { get; set; } = true;
}

public readonly record struct Result(bool Ok, string? Message, string? Id);

public sealed record UserPick(string UserId);

public sealed record Decision(bool Approve, string? Comment = null);
