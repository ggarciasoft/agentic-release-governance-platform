namespace ReleaseAssistant.Domain.Entities;

public class ReleaseWorkItem : EntityBase
{
    public Guid ReleaseId { get; set; }
    public int AzureDevOpsWorkItemId { get; set; }
    public string WorkItemType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public string TagsJson { get; set; } = "[]";
    public string AreaPath { get; set; } = string.Empty;
    public string IterationPath { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string RawJson { get; set; } = "{}";

    public Release Release { get; set; } = null!;
    public ICollection<ReleasePullRequestWorkItem> PullRequestLinks { get; set; } = new List<ReleasePullRequestWorkItem>();
}
