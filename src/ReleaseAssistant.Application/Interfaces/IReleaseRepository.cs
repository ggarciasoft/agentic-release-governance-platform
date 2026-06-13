using ReleaseAssistant.Domain.Entities;

namespace ReleaseAssistant.Application.Interfaces;

public interface IReleaseRepository
{
    Task<Release?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Release?> GetByIdWithAllDataAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Release>> ListAsync(CancellationToken ct = default);
    Task AddAsync(Release release, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
