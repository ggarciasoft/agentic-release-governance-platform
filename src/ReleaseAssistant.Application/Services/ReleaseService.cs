using System.Text.Json;
using ReleaseAssistant.Application.Interfaces;
using ReleaseAssistant.Application.Models.Mcp;
using ReleaseAssistant.Application.Models.Requests;
using ReleaseAssistant.Application.Models.Responses;
using ReleaseAssistant.Domain.Entities;
using ReleaseAssistant.Domain.Enums;

namespace ReleaseAssistant.Application.Services;

public class ReleaseService(
    IReleaseRepository releases,
    IApplicationMappingRepository mappings,
    IValidationEngine validationEngine)
{
    public async Task<Release> CreateAsync(CreateReleaseRequest req, CancellationToken ct = default)
    {
        var release = new Release
        {
            Name = req.ReleaseName,
            ChangeRequest = req.ChangeRequest,
            Organization = req.Organization,
            Project = req.Project,
            TargetEnvironment = req.TargetEnvironment,
            Status = ReleaseStatus.Created
        };

        foreach (var appName in req.Applications)
        {
            var mapping = await mappings.GetByNameAsync(appName, req.Organization, req.Project, ct);
            release.Applications.Add(new ReleaseApplication
            {
                ReleaseId = release.Id,
                ApplicationName = appName,
                RepositoryName = mapping?.RepositoryName ?? string.Empty,
                BuildDefinitionId = mapping?.BuildDefinitionId,
                ReleaseDefinitionId = mapping?.ReleaseDefinitionId,
                ProductionEnvironmentName = mapping?.ProductionEnvironmentName ?? req.TargetEnvironment,
                UatEnvironmentName = mapping?.UatEnvironmentName ?? "UAT"
            });
        }

        await releases.AddAsync(release, ct);
        await releases.SaveChangesAsync(ct);
        return release;
    }

    public async Task<Release?> GetAsync(Guid id, CancellationToken ct = default)
        => await releases.GetByIdWithAllDataAsync(id, ct);

    public async Task<IReadOnlyList<Release>> ListAsync(CancellationToken ct = default)
        => await releases.ListAsync(ct);

    public async Task<int> AttachWorkItemsAsync(AttachWorkItemsRequest req, CancellationToken ct = default)
    {
        if (!Guid.TryParse(req.ReleaseId, out var releaseId))
            throw new ArgumentException("Invalid releaseId.", nameof(req));

        var release = await releases.GetByIdAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {req.ReleaseId} not found.");

        int count = 0;
        foreach (var wi in req.WorkItems)
        {
            release.WorkItems.Add(new ReleaseWorkItem
            {
                ReleaseId = releaseId,
                AzureDevOpsWorkItemId = wi.Id,
                WorkItemType = wi.Type,
                Title = wi.Title,
                State = wi.State,
                AssignedTo = wi.AssignedTo,
                TagsJson = JsonSerializer.Serialize(wi.Tags ?? Array.Empty<string>()),
                AreaPath = wi.AreaPath,
                IterationPath = wi.IterationPath,
                Url = wi.Url,
                RawJson = wi.RawJson
            });
            count++;
        }

        release.Status = ReleaseStatus.Analyzing;
        release.UpdatedAt = DateTime.UtcNow;
        await releases.SaveChangesAsync(ct);
        return count;
    }

    public async Task<int> AttachPullRequestsAsync(AttachPullRequestsRequest req, CancellationToken ct = default)
    {
        if (!Guid.TryParse(req.ReleaseId, out var releaseId))
            throw new ArgumentException("Invalid releaseId.", nameof(req));

        var release = await releases.GetByIdWithAllDataAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {req.ReleaseId} not found.");

        int count = 0;
        foreach (var pr in req.PullRequests)
        {
            var releasePr = new ReleasePullRequest
            {
                ReleaseId = releaseId,
                AzureDevOpsPullRequestId = pr.PullRequestId,
                RepositoryId = pr.RepositoryId,
                RepositoryName = pr.RepositoryName,
                Title = pr.Title,
                Status = pr.Status,
                SourceBranch = pr.SourceBranch,
                TargetBranch = pr.TargetBranch,
                CreatedBy = pr.CreatedBy,
                CompletedBy = pr.CompletedBy,
                CreatedAtFromAzureDevOps = pr.CreatedAt,
                CompletedAtFromAzureDevOps = pr.CompletedAt,
                Url = pr.Url,
                RawJson = pr.RawJson
            };

            foreach (var wiId in pr.LinkedWorkItemIds ?? Array.Empty<int>())
            {
                var wi = release.WorkItems.FirstOrDefault(w => w.AzureDevOpsWorkItemId == wiId);
                if (wi != null)
                {
                    releasePr.WorkItemLinks.Add(new ReleasePullRequestWorkItem
                    {
                        ReleasePullRequestId = releasePr.Id,
                        ReleaseWorkItemId = wi.Id,
                        AzureDevOpsWorkItemId = wiId,
                        AzureDevOpsPullRequestId = pr.PullRequestId
                    });
                }
            }

            release.PullRequests.Add(releasePr);
            count++;
        }

        release.UpdatedAt = DateTime.UtcNow;
        await releases.SaveChangesAsync(ct);
        return count;
    }

    public async Task<int> AttachDeploymentsAsync(AttachDeploymentsRequest req, CancellationToken ct = default)
    {
        if (!Guid.TryParse(req.ReleaseId, out var releaseId))
            throw new ArgumentException("Invalid releaseId.", nameof(req));

        var release = await releases.GetByIdAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {req.ReleaseId} not found.");

        int count = 0;
        foreach (var dep in req.Deployments)
        {
            release.Deployments.Add(new ReleaseDeployment
            {
                ReleaseId = releaseId,
                ApplicationName = dep.ApplicationName,
                AzureDevOpsReleaseName = dep.ReleaseName,
                AzureDevOpsReleaseId = dep.AzureDevOpsReleaseId,
                ReleaseDefinitionId = dep.ReleaseDefinitionId,
                ReleaseDefinitionName = dep.ReleaseDefinitionName,
                EnvironmentName = dep.EnvironmentName,
                DeploymentStatus = dep.Status,
                ApprovalStatus = dep.ApprovalStatus,
                DeploymentUrl = dep.Url,
                StartedAt = dep.StartedAt,
                CompletedAt = dep.CompletedAt,
                RawJson = dep.RawJson,
                IsCurrentDeployment = true
            });
            count++;
        }

        release.Status = ReleaseStatus.AnalysisComplete;
        release.UpdatedAt = DateTime.UtcNow;
        await releases.SaveChangesAsync(ct);
        return count;
    }

    public async Task<IReadOnlyList<ReleaseRollbackCandidate>> FindRollbackCandidatesAsync(
        Guid releaseId, CancellationToken ct = default)
    {
        var release = await releases.GetByIdWithAllDataAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        var candidates = new List<ReleaseRollbackCandidate>();

        foreach (var app in release.Applications)
        {
            var currentDeployment = release.Deployments
                .FirstOrDefault(d => d.ApplicationName.Equals(app.ApplicationName, StringComparison.OrdinalIgnoreCase)
                    && d.IsCurrentDeployment);

            if (currentDeployment == null) continue;

            // Rollback candidate: latest successful deployment before the current candidate
            // In the MVP this logic is simplified — we look at existing rollback data
            // A more complete implementation would call AzureDevOps to find the prior deployment.
            // For now, we mark that no rollback candidate was found if one isn't already attached.
            var existing = release.RollbackCandidates
                .FirstOrDefault(r => r.ApplicationName.Equals(app.ApplicationName, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
                candidates.Add(existing);
        }

        return candidates;
    }

    public async Task<ValidationSummary> ValidateAsync(Guid releaseId, CancellationToken ct = default)
    {
        var release = await releases.GetByIdWithAllDataAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        var summary = await validationEngine.ValidateAsync(release, ct);

        // Persist results
        release.ValidationResults.Clear();
        foreach (var f in summary.Blockers.Concat(summary.Warnings).Concat(summary.Info))
        {
            release.ValidationResults.Add(new ReleaseValidationResult
            {
                ReleaseId = releaseId,
                RuleCode = f.Code,
                Severity = f.Severity,
                Status = summary.Status,
                Message = f.Message,
                EntityType = f.EntityType,
                EntityId = f.EntityId
            });
        }

        release.Status = ReleaseStatus.ValidationComplete;
        release.UpdatedAt = DateTime.UtcNow;
        await releases.SaveChangesAsync(ct);
        return summary;
    }

    public async Task<ReleasePackage> GeneratePackageAsync(Guid releaseId, CancellationToken ct = default)
    {
        var release = await releases.GetByIdWithAllDataAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        return new ReleasePackage(
            Release: new ReleasePackageRelease(
                release.Id.ToString(), release.Name, release.ChangeRequest,
                release.Organization, release.Project, release.TargetEnvironment, release.Status.ToString()),
            Applications: release.Applications.Select(a => new ReleasePackageApplication(
                a.ApplicationName, a.RepositoryName, a.ProductionEnvironmentName)).ToList(),
            WorkItems: release.WorkItems.Select(w => new ReleasePackageWorkItem(
                w.AzureDevOpsWorkItemId, w.WorkItemType, w.Title, w.State, w.AssignedTo, w.Url)).ToList(),
            PullRequests: release.PullRequests.Select(p => new ReleasePackagePullRequest(
                p.AzureDevOpsPullRequestId, p.RepositoryName, p.Title, p.Status, p.TargetBranch, p.Url)).ToList(),
            Deployments: release.Deployments.Select(d => new ReleasePackageDeployment(
                d.ApplicationName, d.AzureDevOpsReleaseName, d.EnvironmentName, d.DeploymentStatus, d.ApprovalStatus, d.DeploymentUrl)).ToList(),
            RollbackCandidates: release.RollbackCandidates.Select(r => new ReleasePackageRollbackCandidate(
                r.ApplicationName, r.AzureDevOpsReleaseName, r.EnvironmentName, r.DeploymentStatus, r.RollbackUrl)).ToList(),
            Validations: release.ValidationResults.Select(v => new ReleasePackageValidation(
                v.RuleCode, v.Severity.ToString(), v.Message)).ToList());
    }

    public async Task<ReleaseDocument> SaveDocumentAsync(Guid releaseId, string format, string content,
        string generatedBy = "agent", CancellationToken ct = default)
    {
        var release = await releases.GetByIdAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        var version = release.Documents.Count + 1;
        var doc = new ReleaseDocument
        {
            ReleaseId = releaseId,
            Format = format,
            Content = content,
            Version = version,
            GeneratedBy = generatedBy,
            GeneratedAt = DateTime.UtcNow
        };

        release.Documents.Add(doc);
        release.Status = ReleaseStatus.DocumentGenerated;
        release.UpdatedAt = DateTime.UtcNow;
        await releases.SaveChangesAsync(ct);
        return doc;
    }

    public async Task SoftDeleteAsync(Guid releaseId, CancellationToken ct = default)
    {
        var release = await releases.GetByIdAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");
        release.DeletedAt = DateTime.UtcNow;
        release.UpdatedAt = DateTime.UtcNow;
        await releases.SaveChangesAsync(ct);
    }
}
