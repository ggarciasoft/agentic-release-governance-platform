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

        var workItems = req.WorkItems.Select(wi => new ReleaseWorkItem
        {
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
        }).ToList();

        return await releases.AddWorkItemsAsync(releaseId, workItems, ct);
    }

    public async Task<int> AttachPullRequestsAsync(AttachPullRequestsRequest req, CancellationToken ct = default)
    {
        if (!Guid.TryParse(req.ReleaseId, out var releaseId))
            throw new ArgumentException("Invalid releaseId.", nameof(req));

        var release = await releases.GetByIdWithAllDataAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {req.ReleaseId} not found.");

        var pullRequests = new List<ReleasePullRequest>();
        foreach (var pr in req.PullRequests)
        {
            var releasePr = new ReleasePullRequest
            {
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
                        ReleaseWorkItemId = wi.Id,
                        AzureDevOpsWorkItemId = wiId,
                        AzureDevOpsPullRequestId = pr.PullRequestId
                    });
                }
            }

            pullRequests.Add(releasePr);
        }

        return await releases.AddPullRequestsAsync(releaseId, pullRequests, ct);
    }

    public async Task<int> AttachDeploymentsAsync(AttachDeploymentsRequest req, CancellationToken ct = default)
    {
        if (!Guid.TryParse(req.ReleaseId, out var releaseId))
            throw new ArgumentException("Invalid releaseId.", nameof(req));

        // Deduplicate by ApplicationName — last wins
        var deduped = req.Deployments
            .GroupBy(d => d.ApplicationName, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.Last());

        var deployments = deduped.Select(dep => new ReleaseDeployment
        {
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
        }).ToList();

        return await releases.AddDeploymentsAsync(releaseId, deployments, ct);
    }

    public async Task<int> AttachRollbackCandidatesAsync(AttachRollbackCandidatesRequest req, CancellationToken ct = default)
    {
        if (!Guid.TryParse(req.ReleaseId, out var releaseId))
            throw new ArgumentException("Invalid releaseId.", nameof(req));

        // Deduplicate by ApplicationName — last wins
        var deduped = req.Candidates
            .GroupBy(c => c.ApplicationName, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.Last());

        var candidates = deduped.Select(c => new ReleaseRollbackCandidate
        {
            ApplicationName = c.ApplicationName,
            AzureDevOpsReleaseName = c.ReleaseName,
            AzureDevOpsReleaseId = c.AzureDevOpsReleaseId,
            ReleaseDefinitionId = c.ReleaseDefinitionId,
            ReleaseDefinitionName = c.ReleaseDefinitionName,
            EnvironmentName = c.EnvironmentName,
            DeploymentStatus = c.Status,
            RollbackUrl = c.Url,
            CompletedAt = c.CompletedAt,
            RawJson = c.RawJson
        }).ToList();

        return await releases.AddRollbackCandidatesAsync(releaseId, candidates, ct);
    }

    public async Task<int> SyncApplicationsFromMappingsAsync(Guid releaseId, CancellationToken ct = default)
    {
        var release = await releases.GetByIdWithAllDataAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        var updated = 0;
        foreach (var app in release.Applications)
        {
            var mapping = await mappings.GetByNameAsync(app.ApplicationName, release.Organization, release.Project, ct);
            if (mapping == null)
                continue;

            app.RepositoryName = mapping.RepositoryName;
            app.BuildDefinitionId = mapping.BuildDefinitionId;
            app.ReleaseDefinitionId = mapping.ReleaseDefinitionId;
            app.ProductionEnvironmentName = mapping.ProductionEnvironmentName;
            app.UatEnvironmentName = mapping.UatEnvironmentName;
            app.UpdatedAt = DateTime.UtcNow;
            updated++;
        }

        if (updated > 0)
        {
            release.UpdatedAt = DateTime.UtcNow;
            await releases.SaveChangesAsync(ct);
        }

        return updated;
    }

    public async Task<int> EnsureApplicationsAsync(Guid releaseId, IReadOnlyList<string> applicationNames,
        string organization, string project, CancellationToken ct = default)
    {
        var release = await releases.GetByIdWithAllDataAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        var missing = applicationNames
            .Where(name => !release.Applications.Any(a =>
                a.ApplicationName.Equals(name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (missing.Count == 0) return 0;

        var applications = new List<ReleaseApplication>();
        foreach (var appName in missing)
        {
            var mapping = await mappings.GetByNameAsync(appName, organization, project, ct);
            applications.Add(new ReleaseApplication
            {
                ApplicationName = appName,
                RepositoryName = mapping?.RepositoryName ?? string.Empty,
                BuildDefinitionId = mapping?.BuildDefinitionId,
                ReleaseDefinitionId = mapping?.ReleaseDefinitionId,
                ProductionEnvironmentName = mapping?.ProductionEnvironmentName ?? release.TargetEnvironment,
                UatEnvironmentName = mapping?.UatEnvironmentName ?? "UAT"
            });
        }

        return await releases.AddApplicationsAsync(releaseId, applications, ct);
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

        var results = summary.Blockers.Concat(summary.Warnings).Concat(summary.Info)
            .Select(f => new ReleaseValidationResult
            {
                RuleCode = f.Code,
                Severity = f.Severity,
                Status = summary.Status,
                Message = f.Message,
                EntityType = f.EntityType,
                EntityId = f.EntityId
            }).ToList();

        await releases.SaveValidationResultsAsync(releaseId, summary.Status, results, ct);
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
            Deployments: release.Deployments
                .Where(d => d.IsCurrentDeployment)
                .Select(d => new ReleasePackageDeployment(
                d.ApplicationName, d.AzureDevOpsReleaseName, d.EnvironmentName, d.DeploymentStatus, d.ApprovalStatus, d.DeploymentUrl)).ToList(),
            RollbackCandidates: release.RollbackCandidates.Select(r => new ReleasePackageRollbackCandidate(
                r.ApplicationName, r.AzureDevOpsReleaseName, r.EnvironmentName, r.DeploymentStatus, r.RollbackUrl)).ToList(),
            Validations: release.ValidationResults.Select(v => new ReleasePackageValidation(
                v.RuleCode, v.Severity.ToString(), v.Message)).ToList());
    }

    public async Task<ReleaseDocument> SaveDocumentAsync(Guid releaseId, string format, string content,
        string generatedBy = "agent", CancellationToken ct = default)
    {
        _ = await releases.GetByIdAsync(releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        var doc = new ReleaseDocument
        {
            Format = format,
            Content = content,
            GeneratedBy = generatedBy,
            GeneratedAt = DateTime.UtcNow
        };

        return await releases.AddDocumentAsync(releaseId, doc, ct);
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
