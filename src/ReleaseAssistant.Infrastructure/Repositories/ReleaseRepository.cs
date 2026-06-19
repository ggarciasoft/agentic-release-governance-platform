using Microsoft.EntityFrameworkCore;
using ReleaseAssistant.Application.Interfaces;
using ReleaseAssistant.Domain.Entities;
using ReleaseAssistant.Domain.Enums;
using ReleaseAssistant.Infrastructure.Data;

namespace ReleaseAssistant.Infrastructure.Repositories;

public class ReleaseRepository(AppDbContext db) : IReleaseRepository
{
    public Task<Release?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Releases.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Release?> GetByIdWithAllDataAsync(Guid id, CancellationToken ct = default)
        => db.Releases
            .Include(r => r.Applications)
            .Include(r => r.WorkItems)
            .Include(r => r.PullRequests).ThenInclude(p => p.WorkItemLinks)
            .Include(r => r.Deployments)
            .Include(r => r.RollbackCandidates)
            .Include(r => r.ValidationResults)
            .Include(r => r.Documents)
            .Include(r => r.ToolCallLogs)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Release?> GetByChangeRequestAsync(string changeRequest, string organization, string project,
        CancellationToken ct = default)
        => db.Releases
            .Where(r => r.DeletedAt == null
                && r.ChangeRequest == changeRequest
                && r.Organization == organization
                && r.Project == project)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Release>> ListAsync(CancellationToken ct = default)
        => await db.Releases
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Release release, CancellationToken ct = default)
        => await db.Releases.AddAsync(release, ct);

    public async Task<int> AddWorkItemsAsync(Guid releaseId, IReadOnlyList<ReleaseWorkItem> workItems, CancellationToken ct = default)
    {
        var release = await db.Releases.FirstOrDefaultAsync(r => r.Id == releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        // Deduplicate by Azure DevOps work item ID — last wins within the batch
        var deduped = workItems
            .GroupBy(w => w.AzureDevOpsWorkItemId)
            .Select(g => g.Last())
            .ToList();

        var added = 0;
        foreach (var workItem in deduped)
        {
            var existing = await db.ReleaseWorkItems
                .FirstOrDefaultAsync(w => w.ReleaseId == releaseId
                    && w.AzureDevOpsWorkItemId == workItem.AzureDevOpsWorkItemId, ct);

            if (existing != null)
            {
                UpdateWorkItemSnapshot(existing, workItem);
                continue;
            }

            workItem.ReleaseId = releaseId;
            await db.ReleaseWorkItems.AddAsync(workItem, ct);
            added++;
        }

        release.Status = ReleaseStatus.Analyzing;
        release.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return added;
    }

    public async Task<int> AddPullRequestsAsync(Guid releaseId, IReadOnlyList<ReleasePullRequest> pullRequests, CancellationToken ct = default)
    {
        var release = await db.Releases.FirstOrDefaultAsync(r => r.Id == releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        // Deduplicate by Azure DevOps pull request ID — last wins within the batch
        var deduped = pullRequests
            .GroupBy(p => p.AzureDevOpsPullRequestId)
            .Select(g => g.Last())
            .ToList();

        var added = 0;
        foreach (var pullRequest in deduped)
        {
            var existing = await db.ReleasePullRequests
                .Include(p => p.WorkItemLinks)
                .FirstOrDefaultAsync(p => p.ReleaseId == releaseId
                    && p.AzureDevOpsPullRequestId == pullRequest.AzureDevOpsPullRequestId, ct);

            if (existing != null)
            {
                UpdatePullRequestSnapshot(existing, pullRequest);
                continue;
            }

            pullRequest.ReleaseId = releaseId;
            await db.ReleasePullRequests.AddAsync(pullRequest, ct);
            added++;
        }

        release.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return added;
    }

    public async Task<int> AddDeploymentsAsync(Guid releaseId, IReadOnlyList<ReleaseDeployment> deployments, CancellationToken ct = default)
    {
        var release = await db.Releases.FirstOrDefaultAsync(r => r.Id == releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        // Deduplicate by ApplicationName — last wins within the batch
        var deduped = deployments
            .GroupBy(d => d.ApplicationName, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.Last())
            .ToList();

        foreach (var deployment in deduped)
        {
            // Deactivate any prior current deployments (from DB or already tracked in this batch)
            var existing = await db.ReleaseDeployments
                .Where(d => d.ReleaseId == releaseId
                    && d.ApplicationName == deployment.ApplicationName
                    && d.IsCurrentDeployment)
                .ToListAsync(ct);

            // Also catch entries already tracked by the change tracker from earlier iterations
            foreach (var tracked in db.ChangeTracker.Entries<ReleaseDeployment>()
                .Where(e => e.Entity.ReleaseId == releaseId
                    && e.Entity.ApplicationName.Equals(deployment.ApplicationName, StringComparison.OrdinalIgnoreCase)
                    && e.Entity.IsCurrentDeployment))
            {
                tracked.Entity.IsCurrentDeployment = false;
            }

            foreach (var prior in existing)
                prior.IsCurrentDeployment = false;

            deployment.ReleaseId = releaseId;
            deployment.IsCurrentDeployment = true;
            await db.ReleaseDeployments.AddAsync(deployment, ct);
        }

        release.Status = ReleaseStatus.AnalysisComplete;
        release.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return deduped.Count;
    }

    public async Task<int> AddRollbackCandidatesAsync(Guid releaseId, IReadOnlyList<ReleaseRollbackCandidate> candidates, CancellationToken ct = default)
    {
        var release = await db.Releases.FirstOrDefaultAsync(r => r.Id == releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        // Deduplicate by ApplicationName — last wins within the batch
        var deduped = candidates
            .GroupBy(c => c.ApplicationName, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.Last())
            .ToList();

        foreach (var candidate in deduped)
        {
            // Remove any existing rollback candidate for the same release + app (from DB)
            var existing = await db.ReleaseRollbackCandidates
                .Where(r => r.ReleaseId == releaseId
                    && r.ApplicationName == candidate.ApplicationName)
                .ToListAsync(ct);

            // Also remove entries already tracked by the change tracker from earlier iterations
            foreach (var tracked in db.ChangeTracker.Entries<ReleaseRollbackCandidate>()
                .Where(e => e.Entity.ReleaseId == releaseId
                    && e.Entity.ApplicationName.Equals(candidate.ApplicationName, StringComparison.OrdinalIgnoreCase)))
            {
                tracked.State = EntityState.Detached;
            }

            db.ReleaseRollbackCandidates.RemoveRange(existing);

            candidate.ReleaseId = releaseId;
            await db.ReleaseRollbackCandidates.AddAsync(candidate, ct);
        }

        release.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return deduped.Count;
    }

    public async Task<int> AddApplicationsAsync(Guid releaseId, IReadOnlyList<ReleaseApplication> applications, CancellationToken ct = default)
    {
        var release = await db.Releases.FirstOrDefaultAsync(r => r.Id == releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        foreach (var application in applications)
        {
            application.ReleaseId = releaseId;
            await db.ReleaseApplications.AddAsync(application, ct);
        }

        release.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return applications.Count;
    }

    public async Task SaveValidationResultsAsync(Guid releaseId, ReadinessStatus status,
        IReadOnlyList<ReleaseValidationResult> results, CancellationToken ct = default)
    {
        var release = await db.Releases.FirstOrDefaultAsync(r => r.Id == releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        var existing = await db.ReleaseValidationResults
            .Where(v => v.ReleaseId == releaseId)
            .ToListAsync(ct);
        db.ReleaseValidationResults.RemoveRange(existing);

        foreach (var result in results)
        {
            result.ReleaseId = releaseId;
            await db.ReleaseValidationResults.AddAsync(result, ct);
        }

        release.Status = ReleaseStatus.ValidationComplete;
        release.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<ReleaseDocument> AddDocumentAsync(Guid releaseId, ReleaseDocument document, CancellationToken ct = default)
    {
        var release = await db.Releases.FirstOrDefaultAsync(r => r.Id == releaseId, ct)
            ?? throw new KeyNotFoundException($"Release {releaseId} not found.");

        document.ReleaseId = releaseId;
        document.Version = await db.ReleaseDocuments.CountAsync(d => d.ReleaseId == releaseId, ct) + 1;
        await db.ReleaseDocuments.AddAsync(document, ct);

        release.Status = ReleaseStatus.DocumentGenerated;
        release.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return document;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);

    private static void UpdateWorkItemSnapshot(ReleaseWorkItem existing, ReleaseWorkItem incoming)
    {
        existing.WorkItemType = incoming.WorkItemType;
        existing.Title = incoming.Title;
        existing.State = incoming.State;
        existing.AssignedTo = incoming.AssignedTo;
        existing.TagsJson = incoming.TagsJson;
        existing.AreaPath = incoming.AreaPath;
        existing.IterationPath = incoming.IterationPath;
        existing.Url = incoming.Url;
        existing.RawJson = incoming.RawJson;
        existing.UpdatedAt = DateTime.UtcNow;
    }

    private static void UpdatePullRequestSnapshot(ReleasePullRequest existing, ReleasePullRequest incoming)
    {
        existing.RepositoryId = incoming.RepositoryId;
        existing.RepositoryName = incoming.RepositoryName;
        existing.Title = incoming.Title;
        existing.Status = incoming.Status;
        existing.SourceBranch = incoming.SourceBranch;
        existing.TargetBranch = incoming.TargetBranch;
        existing.CreatedBy = incoming.CreatedBy;
        existing.CompletedBy = incoming.CompletedBy;
        existing.CreatedAtFromAzureDevOps = incoming.CreatedAtFromAzureDevOps;
        existing.CompletedAtFromAzureDevOps = incoming.CompletedAtFromAzureDevOps;
        existing.Url = incoming.Url;
        existing.RawJson = incoming.RawJson;
        existing.UpdatedAt = DateTime.UtcNow;
    }
}
