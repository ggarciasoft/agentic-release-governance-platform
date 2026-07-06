namespace ReleaseAssistant.Application.Models.Requests;

public record PullRequestData(
    int PullRequestId,
    string RepositoryName,
    string RepositoryId = "",
    string Title = "",
    string Status = "",
    string SourceBranch = "",
    string TargetBranch = "",
    string CreatedBy = "",
    string CompletedBy = "",
    DateTime? CreatedAt = null,
    DateTime? CompletedAt = null,
    string Url = "",
    string RawJson = "{}",
    IReadOnlyList<int>? LinkedWorkItemIds = null,
    string? MergeCommitId = null);

public record AttachPullRequestsRequest(
    string ReleaseId,
    IReadOnlyList<PullRequestData> PullRequests);
