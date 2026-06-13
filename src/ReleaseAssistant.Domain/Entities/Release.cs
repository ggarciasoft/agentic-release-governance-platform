using ReleaseAssistant.Domain.Enums;

namespace ReleaseAssistant.Domain.Entities;

public class Release : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string ChangeRequest { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string TargetEnvironment { get; set; } = string.Empty;
    public ReleaseStatus Status { get; set; } = ReleaseStatus.Created;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? DeletedAt { get; set; }

    public ICollection<ReleaseApplication> Applications { get; set; } = new List<ReleaseApplication>();
    public ICollection<ReleaseWorkItem> WorkItems { get; set; } = new List<ReleaseWorkItem>();
    public ICollection<ReleasePullRequest> PullRequests { get; set; } = new List<ReleasePullRequest>();
    public ICollection<ReleaseDeployment> Deployments { get; set; } = new List<ReleaseDeployment>();
    public ICollection<ReleaseRollbackCandidate> RollbackCandidates { get; set; } = new List<ReleaseRollbackCandidate>();
    public ICollection<ReleaseValidationResult> ValidationResults { get; set; } = new List<ReleaseValidationResult>();
    public ICollection<ReleaseDocument> Documents { get; set; } = new List<ReleaseDocument>();
    public ICollection<AgentRun> AgentRuns { get; set; } = new List<AgentRun>();
    public ICollection<ToolCallLog> ToolCallLogs { get; set; } = new List<ToolCallLog>();
}
