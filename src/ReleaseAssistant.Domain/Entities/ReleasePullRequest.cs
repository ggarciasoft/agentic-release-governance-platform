namespace ReleaseAssistant.Domain.Entities;

public class ReleasePullRequest : EntityBase
{
    public Guid ReleaseId { get; set; }
    public int AzureDevOpsPullRequestId { get; set; }
    public string RepositoryId { get; set; } = string.Empty;
    public string RepositoryName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string SourceBranch { get; set; } = string.Empty;
    public string TargetBranch { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string CompletedBy { get; set; } = string.Empty;
    public DateTime? CreatedAtFromAzureDevOps { get; set; }
    public DateTime? CompletedAtFromAzureDevOps { get; set; }
    public string Url { get; set; } = string.Empty;
    public string RawJson { get; set; } = "{}";
    public string? MergeCommitId { get; set; }

    public Release Release { get; set; } = null!;
    public ICollection<ReleasePullRequestWorkItem> WorkItemLinks { get; set; } = new List<ReleasePullRequestWorkItem>();
}
