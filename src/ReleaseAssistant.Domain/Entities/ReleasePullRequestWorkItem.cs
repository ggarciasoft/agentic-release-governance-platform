namespace ReleaseAssistant.Domain.Entities;

public class ReleasePullRequestWorkItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReleasePullRequestId { get; set; }
    public Guid ReleaseWorkItemId { get; set; }
    public int AzureDevOpsWorkItemId { get; set; }
    public int AzureDevOpsPullRequestId { get; set; }

    public ReleasePullRequest PullRequest { get; set; } = null!;
    public ReleaseWorkItem WorkItem { get; set; } = null!;
}
