using Microsoft.EntityFrameworkCore;
using ReleaseAssistant.Application.Interfaces;
using ReleaseAssistant.Domain.Entities;
using ReleaseAssistant.Infrastructure.Data;

namespace ReleaseAssistant.Infrastructure.Repositories;

public class ApplicationMappingRepository(AppDbContext db) : IApplicationMappingRepository
{
    public Task<ApplicationMapping?> GetByNameAsync(string applicationName, string? organization = null,
        string? project = null, CancellationToken ct = default)
    {
        var query = db.ApplicationMappings
            .Where(m => m.IsActive && m.ApplicationName == applicationName);

        if (!string.IsNullOrWhiteSpace(organization))
            query = query.Where(m => m.Organization == organization);
        if (!string.IsNullOrWhiteSpace(project))
            query = query.Where(m => m.Project == project);

        return query.FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<ApplicationMapping>> ListAsync(CancellationToken ct = default)
        => await db.ApplicationMappings.Where(m => m.IsActive).ToListAsync(ct);

    public Task<ApplicationMapping?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.ApplicationMappings.FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task AddAsync(ApplicationMapping mapping, CancellationToken ct = default)
        => await db.ApplicationMappings.AddAsync(mapping, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
