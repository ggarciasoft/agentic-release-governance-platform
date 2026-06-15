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
        string organization, string project, CancellationToken ct = default);

    Task<IReadOnlyList<RollbackCandidateData>> CollectRollbackCandidatesAsync(
        IReadOnlyList<(string ApplicationName, int ReleaseDefinitionId, string EnvironmentName, int? CurrentReleaseId)> applications,
        string organization, string project, CancellationToken ct = default);
}
