using ReleaseAssistant.Domain.Entities;

namespace ReleaseAssistant.Application.Interfaces;

public interface IApplicationMappingRepository
{
    Task<ApplicationMapping?> GetByNameAsync(string applicationName, string? organization = null, string? project = null, CancellationToken ct = default);
    Task<IReadOnlyList<ApplicationMapping>> ListAsync(CancellationToken ct = default);
    Task<ApplicationMapping?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ApplicationMapping mapping, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
