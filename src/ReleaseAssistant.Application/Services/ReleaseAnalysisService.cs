using ReleaseAssistant.Application.Interfaces;
using ReleaseAssistant.Application.Models.Requests;
using ReleaseAssistant.Application.Models.Responses;

namespace ReleaseAssistant.Application.Services;

public class ReleaseAnalysisService(
    ReleaseService releaseService,
    IAzureDevOpsDataCollector adoCollector,
    IAnalysisStatusStore statusStore)
{
    public AnalysisStatusResponse? GetAnalysisStatus(Guid releaseId) =>
        statusStore.GetStatus(releaseId);

    public async Task<FullAnalysisResult> AnalyzeAllAsync(Guid releaseId, CancellationToken ct = default)
    {
        var warnings = new List<string>();
        var steps = new List<AnalysisStepResult>();

        UpdateStatus(releaseId, "CollectingWorkItems", 10, warnings);
        steps.Add(await AnalyzeWorkItemsAsync(releaseId, ct));
        warnings.AddRange(steps[^1].Warnings);

        UpdateStatus(releaseId, "CollectingPullRequests", 35, warnings);
        steps.Add(await AnalyzePullRequestsAsync(releaseId, ct));
        warnings.AddRange(steps[^1].Warnings);

        UpdateStatus(releaseId, "CollectingDeployments", 60, warnings);
        steps.Add(await AnalyzeDeploymentsAsync(releaseId, ct));
        warnings.AddRange(steps[^1].Warnings);

        UpdateStatus(releaseId, "CollectingRollbackCandidates", 85, warnings);
        steps.Add(await AnalyzeRollbackAsync(releaseId, ct));
        warnings.AddRange(steps[^1].Warnings);

        UpdateStatus(releaseId, "Completed", 100, warnings);
        return new FullAnalysisResult(releaseId.ToString(), "Completed", steps, warnings);
    }

    public async Task<AnalysisStepResult> AnalyzeWorkItemsAsync(Guid releaseId, CancellationToken ct = default)
    {
        var release = await releaseService.GetAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        var warnings = new List<string>();
        if (string.IsNullOrWhiteSpace(release.ChangeRequest))
        {
            warnings.Add("Release has no changeRequest; cannot search work items by tag.");
            return new AnalysisStepResult("work-items", 0, warnings);
        }

        var workItems = await adoCollector.CollectWorkItemsByTagAsync(
            release.ChangeRequest, release.Organization, release.Project, ct);

        if (workItems.Count == 0)
            warnings.Add($"No work items found with tag '{release.ChangeRequest}'.");

        var count = workItems.Count == 0
            ? 0
            : await releaseService.AttachWorkItemsAsync(
                new AttachWorkItemsRequest(releaseId.ToString(), workItems), ct);

        return new AnalysisStepResult("work-items", count, warnings);
    }

    public async Task<AnalysisStepResult> AnalyzePullRequestsAsync(Guid releaseId, CancellationToken ct = default)
    {
        var release = await releaseService.GetAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        var warnings = new List<string>();
        if (release.WorkItems.Count == 0)
        {
            warnings.Add("No work items attached; run analyze/work-items first.");
            return new AnalysisStepResult("pull-requests", 0, warnings);
        }

        var workItemData = release.WorkItems.Select(w => new WorkItemData(
            w.AzureDevOpsWorkItemId, w.WorkItemType, w.Title, w.State, w.AssignedTo,
            Tags: null, w.AreaPath, w.IterationPath, w.Url, w.RawJson)).ToList();

        var pullRequests = await adoCollector.CollectPullRequestsForWorkItemsAsync(
            workItemData, release.Organization, release.Project, ct);

        if (pullRequests.Count == 0)
            warnings.Add("No pull requests found linked to attached work items.");

        var count = pullRequests.Count == 0
            ? 0
            : await releaseService.AttachPullRequestsAsync(
                new AttachPullRequestsRequest(releaseId.ToString(), pullRequests), ct);

        return new AnalysisStepResult("pull-requests", count, warnings);
    }

    public async Task<AnalysisStepResult> AnalyzeDeploymentsAsync(Guid releaseId, CancellationToken ct = default)
    {
        var release = await releaseService.GetAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        var warnings = new List<string>();
        await releaseService.SyncApplicationsFromMappingsAsync(releaseId, ct);
        release = await releaseService.GetAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        var apps = release.Applications
            .Where(a => a.ReleaseDefinitionId.HasValue)
            .Select(a => (
                a.ApplicationName,
                a.ReleaseDefinitionId!.Value,
                string.IsNullOrWhiteSpace(a.ProductionEnvironmentName)
                    ? release.TargetEnvironment
                    : a.ProductionEnvironmentName))
            .ToList();

        foreach (var app in release.Applications.Where(a => !a.ReleaseDefinitionId.HasValue))
            warnings.Add($"{app.ApplicationName} has no releaseDefinitionId mapping; deployment skipped.");

        if (apps.Count == 0)
            return new AnalysisStepResult("deployments", 0, warnings);

        var deployments = await adoCollector.CollectCurrentDeploymentsAsync(
            apps, release.Organization, release.Project, ct);

        if (deployments.Count == 0)
            warnings.Add("No deployment candidates found in Azure DevOps.");

        var count = deployments.Count == 0
            ? 0
            : await releaseService.AttachDeploymentsAsync(
                new AttachDeploymentsRequest(releaseId.ToString(), deployments), ct);

        return new AnalysisStepResult("deployments", count, warnings);
    }

    public async Task<AnalysisStepResult> AnalyzeRollbackAsync(Guid releaseId, CancellationToken ct = default)
    {
        var release = await releaseService.GetAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        var warnings = new List<string>();
        await releaseService.SyncApplicationsFromMappingsAsync(releaseId, ct);
        release = await releaseService.GetAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        var apps = release.Applications
            .Where(a => a.ReleaseDefinitionId.HasValue)
            .Select(a =>
            {
                var env = string.IsNullOrWhiteSpace(a.ProductionEnvironmentName)
                    ? release.TargetEnvironment
                    : a.ProductionEnvironmentName;
                var current = release.Deployments.FirstOrDefault(d =>
                    d.ApplicationName.Equals(a.ApplicationName, StringComparison.OrdinalIgnoreCase)
                    && d.IsCurrentDeployment);
                return (a.ApplicationName, a.ReleaseDefinitionId!.Value, env, current?.AzureDevOpsReleaseId);
            })
            .ToList();

        foreach (var app in release.Applications.Where(a => !a.ReleaseDefinitionId.HasValue))
            warnings.Add($"{app.ApplicationName} has no releaseDefinitionId mapping; rollback skipped.");

        if (apps.Count == 0)
            return new AnalysisStepResult("rollback", 0, warnings);

        var candidates = await adoCollector.CollectRollbackCandidatesAsync(
            apps, release.Organization, release.Project, ct);

        foreach (var app in apps.Where(a => !candidates.Any(c =>
            c.ApplicationName.Equals(a.ApplicationName, StringComparison.OrdinalIgnoreCase))))
            warnings.Add($"{app.ApplicationName} has no rollback candidate in Azure DevOps.");

        var count = candidates.Count == 0
            ? 0
            : await releaseService.AttachRollbackCandidatesAsync(
                new AttachRollbackCandidatesRequest(releaseId.ToString(), candidates), ct);

        return new AnalysisStepResult("rollback", count, warnings);
    }

    private void UpdateStatus(Guid releaseId, string status, int progress, IReadOnlyList<string> warnings) =>
        statusStore.SetStatus(releaseId, new AnalysisStatusResponse(
            releaseId.ToString(), status, progress, DateTime.UtcNow, warnings));
}
