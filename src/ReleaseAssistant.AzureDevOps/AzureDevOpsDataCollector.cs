using System.Text.Json;
using ReleaseAssistant.Application.Interfaces;
using ReleaseAssistant.Application.Models.Requests;
using ReleaseAssistant.AzureDevOps.Models;

namespace ReleaseAssistant.AzureDevOps;

public sealed class AzureDevOpsDataCollector(AzureDevOpsClient client) : IAzureDevOpsDataCollector
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<WorkItemData>> CollectWorkItemsByTagAsync(
        string tag, string organization, string project, CancellationToken ct = default)
    {
        var items = await client.SearchWorkItemsByTagAsync(tag, organization, project, ct);
        return items.Select(MapWorkItem).ToList();
    }

    public async Task<IReadOnlyList<PullRequestData>> CollectPullRequestsForWorkItemsAsync(
        IReadOnlyList<WorkItemData> workItems, string organization, string project,
        CancellationToken ct = default)
    {
        var ids = workItems.Select(w => w.Id).Distinct().ToList();
        var adoWorkItems = await client.GetWorkItemsByIdsAsync(ids, organization, project, ct);

        var results = new Dictionary<int, PullRequestData>();
        foreach (var adoWorkItem in adoWorkItems)
        {
            var prs = await client.GetPullRequestsForWorkItemAsync(adoWorkItem, organization, project, ct);
            foreach (var pr in prs)
            {
                if (results.ContainsKey(pr.PullRequestId)) continue;
                results[pr.PullRequestId] = MapPullRequest(pr, adoWorkItem.Id);
            }
        }

        return results.Values.ToList();
    }

    public async Task<IReadOnlyList<DeploymentData>> CollectCurrentDeploymentsAsync(
        IReadOnlyList<(string ApplicationName, int ReleaseDefinitionId, string EnvironmentName)> applications,
        string organization, string project,
        IReadOnlyCollection<string>? mergeCommits = null,
        CancellationToken ct = default)
    {
        var deployments = new List<DeploymentData>();
        foreach (var (applicationName, releaseDefinitionId, environmentName) in applications)
        {
            var match = await client.FindLatestEnvironmentDeploymentAsync(
                releaseDefinitionId, environmentName, mergeCommits, organization, project, ct);
            if (match == null) continue;

            deployments.Add(MapDeployment(applicationName, match));
        }

        return deployments;
    }

    public async Task<IReadOnlyList<DeploymentData>> CollectDeploymentsFromWorkItemLinksAsync(
        IReadOnlyList<int> workItemIds,
        IReadOnlyList<(string ApplicationName, int ReleaseDefinitionId, string EnvironmentName)> applications,
        string organization, string project,
        CancellationToken ct = default)
    {
        if (workItemIds.Count == 0 || applications.Count == 0)
            return Array.Empty<DeploymentData>();

        var adoWorkItems = await client.GetWorkItemsByIdsAsync(workItemIds, organization, project, ct);

        var releaseIds = adoWorkItems
            .SelectMany(w => w.Relations ?? [])
            .Where(r => r.Rel.Equals("ArtifactLink", StringComparison.OrdinalIgnoreCase))
            .Select(r => AzureDevOpsClient.ExtractReleaseIdFromArtifactUrl(r.Url))
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (releaseIds.Count == 0)
            return Array.Empty<DeploymentData>();

        var releases = new List<AdoRelease>();
        foreach (var rid in releaseIds)
        {
            var rel = await client.GetReleaseByIdAsync(rid, organization, project, ct);
            if (rel != null) releases.Add(rel);
        }

        var results = new List<DeploymentData>();
        foreach (var (applicationName, releaseDefinitionId, environmentName) in applications)
        {
            // Pick the latest release for this definition across all work items (highest ID = most recent).
            var latest = releases
                .Where(r => r.ReleaseDefinition?.Id == releaseDefinitionId)
                .OrderByDescending(r => r.Id)
                .FirstOrDefault();

            if (latest == null) continue;

            var env = latest.Environments?.FirstOrDefault(e =>
                e.Name.Equals(environmentName, StringComparison.OrdinalIgnoreCase));
            if (env == null) continue;

            var status = env.Status ?? env.DeploySteps?.LastOrDefault()?.Status ?? string.Empty;
            var completedAt = env.DeploySteps?.LastOrDefault()?.LastModifiedOn;
            var url = latest.Links?.Web?.Href ?? string.Empty;
            var envDeployment = new AdoEnvironmentDeployment(latest, env, status, url, completedAt, CommitMatched: true);
            results.Add(MapDeployment(applicationName, envDeployment));
        }

        return results;
    }

    public async Task<IReadOnlyList<RollbackCandidateData>> CollectRollbackCandidatesAsync(
        IReadOnlyList<(string ApplicationName, int ReleaseDefinitionId, string EnvironmentName, int? CurrentReleaseId)> applications,
        string organization, string project, CancellationToken ct = default)
    {
        var candidates = new List<RollbackCandidateData>();
        foreach (var (applicationName, releaseDefinitionId, environmentName, currentReleaseId) in applications)
        {
            var match = await client.FindPriorSuccessfulDeploymentAsync(
                releaseDefinitionId, environmentName, currentReleaseId, organization, project, ct);
            if (match == null) continue;

            candidates.Add(MapRollback(applicationName, match));
        }

        return candidates;
    }

    private static WorkItemData MapWorkItem(AdoWorkItem item)
    {
        var tags = (item.Fields.Tags ?? string.Empty)
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        return new WorkItemData(
            item.Id,
            item.Fields.WorkItemType,
            item.Fields.Title,
            item.Fields.State,
            item.Fields.AssignedTo?.DisplayName ?? string.Empty,
            tags,
            item.Fields.AreaPath ?? string.Empty,
            item.Fields.IterationPath ?? string.Empty,
            item.Url,
            JsonSerializer.Serialize(item, JsonOpts));
    }

    private static PullRequestData MapPullRequest(AdoPullRequest pr, int linkedWorkItemId) =>
        new(
            pr.PullRequestId,
            pr.Repository?.Name ?? string.Empty,
            pr.Repository?.Id ?? string.Empty,
            pr.Title,
            pr.Status,
            pr.SourceRefName,
            pr.TargetRefName,
            pr.CreatedBy?.DisplayName ?? string.Empty,
            pr.ClosedBy?.DisplayName ?? string.Empty,
            pr.CreationDate,
            pr.ClosedDate,
            pr.Url,
            JsonSerializer.Serialize(pr, JsonOpts),
            [linkedWorkItemId],
            MergeCommitId: pr.LastMergeSourceCommit?.CommitId);

    private static DeploymentData MapDeployment(string applicationName, AdoEnvironmentDeployment match) =>
        new(
            applicationName,
            match.Release.Name,
            match.Environment.Name,
            match.EnvironmentStatus,
            match.ReleaseUrl,
            match.Release.Id,
            match.Release.ReleaseDefinition?.Id,
            match.Release.ReleaseDefinition?.Name ?? string.Empty,
            ApprovalStatus: string.Empty,
            StartedAt: null,
            CompletedAt: match.CompletedAt,
            JsonSerializer.Serialize(match, JsonOpts),
            CommitMatched: match.CommitMatched);

    private static RollbackCandidateData MapRollback(string applicationName, AdoEnvironmentDeployment match) =>
        new(
            applicationName,
            match.Release.Name,
            match.Environment.Name,
            match.EnvironmentStatus,
            match.ReleaseUrl,
            match.Release.Id,
            match.Release.ReleaseDefinition?.Id,
            match.Release.ReleaseDefinition?.Name ?? string.Empty,
            match.CompletedAt,
            JsonSerializer.Serialize(match, JsonOpts));
}
