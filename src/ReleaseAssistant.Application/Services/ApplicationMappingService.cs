using ReleaseAssistant.Application.Interfaces;
using ReleaseAssistant.Domain.Entities;

namespace ReleaseAssistant.Application.Services;

public class ApplicationMappingService(IApplicationMappingRepository repository)
{
    public Task<ApplicationMapping?> GetByNameAsync(string name, string? org = null, string? project = null,
        CancellationToken ct = default)
        => repository.GetByNameAsync(name, org, project, ct);

    public Task<IReadOnlyList<ApplicationMapping>> ListAsync(CancellationToken ct = default)
        => repository.ListAsync(ct);

    public Task<ApplicationMapping?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => repository.GetByIdAsync(id, ct);

    public async Task<ApplicationMapping> CreateAsync(ApplicationMapping mapping, CancellationToken ct = default)
    {
        await repository.AddAsync(mapping, ct);
        await repository.SaveChangesAsync(ct);
        return mapping;
    }

    public async Task UpdateAsync(ApplicationMapping mapping, CancellationToken ct = default)
    {
        mapping.UpdatedAt = DateTime.UtcNow;
        await repository.SaveChangesAsync(ct);
    }
}
