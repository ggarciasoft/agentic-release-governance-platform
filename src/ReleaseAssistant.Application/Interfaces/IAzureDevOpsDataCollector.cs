using ReleaseAssistant.Application.Models.Requests;

namespace ReleaseAssistant.Application.Interfaces;

public interface IAzureDevOpsDataCollector
{
    Task<IReadOnlyList<WorkItemData>> CollectWorkItemsByTagAsync(
        string tag, string organization, string project, CancellationToken ct = default);

    Task<IReadOnlyList<PullRequestData>> CollectPullRequestsForWorkItemsAsync(
        IReadOnlyList<WorkItemData> workItems, string organization, string project,
        CancellationToken ct = default);

    Task<IReadOnlyList<DeploymentData>> CollectCurrentDeploymentsAsync(
        IReadOnlyList<(string ApplicationName, int ReleaseDefinitionId, string EnvironmentName)> applications,
        string organization, string project,
        IReadOnlyCollection<string>? mergeCommits = null,
        CancellationToken ct = default);

    /// <summary>
    /// Collects deployment candidates from the ArtifactLink relations on work items (populated
    /// when "deployment status reporting for Boards/Work" is enabled in Azure DevOps).
    /// For each application, the latest release (by release ID descending) across all linked
    /// work items is returned.
    /// </summary>
    Task<IReadOnlyList<DeploymentData>> CollectDeploymentsFromWorkItemLinksAsync(
        IReadOnlyList<int> workItemIds,
        IReadOnlyList<(string ApplicationName, int ReleaseDefinitionId, string EnvironmentName)> applications,
        string organization, string project,
        CancellationToken ct = default);

    Task<IReadOnlyList<RollbackCandidateData>> CollectRollbackCandidatesAsync(
        IReadOnlyList<(string ApplicationName, int ReleaseDefinitionId, string EnvironmentName, int? CurrentReleaseId)> applications,
        string organization, string project, CancellationToken ct = default);
}
