using ReleaseAssistant.Domain.Entities;
using ReleaseAssistant.Domain.Enums;

namespace ReleaseAssistant.Application.Interfaces;

public interface IReleaseRepository
{
    Task<Release?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Release?> GetByIdWithAllDataAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Release>> ListAsync(CancellationToken ct = default);
    Task AddAsync(Release release, CancellationToken ct = default);
    Task<int> AddWorkItemsAsync(Guid releaseId, IReadOnlyList<ReleaseWorkItem> workItems, CancellationToken ct = default);
    Task<int> AddPullRequestsAsync(Guid releaseId, IReadOnlyList<ReleasePullRequest> pullRequests, CancellationToken ct = default);
    Task<int> AddDeploymentsAsync(Guid releaseId, IReadOnlyList<ReleaseDeployment> deployments, CancellationToken ct = default);
    Task<int> AddRollbackCandidatesAsync(Guid releaseId, IReadOnlyList<ReleaseRollbackCandidate> candidates, CancellationToken ct = default);
    Task<int> AddApplicationsAsync(Guid releaseId, IReadOnlyList<ReleaseApplication> applications, CancellationToken ct = default);
    Task SaveValidationResultsAsync(Guid releaseId, ReadinessStatus status, IReadOnlyList<ReleaseValidationResult> results, CancellationToken ct = default);
    Task<ReleaseDocument> AddDocumentAsync(Guid releaseId, ReleaseDocument document, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
