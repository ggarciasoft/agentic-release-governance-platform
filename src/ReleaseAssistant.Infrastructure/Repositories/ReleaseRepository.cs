using Microsoft.EntityFrameworkCore;
using ReleaseAssistant.Application.Interfaces;
using ReleaseAssistant.Domain.Entities;
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

    public async Task<IReadOnlyList<Release>> ListAsync(CancellationToken ct = default)
        => await db.Releases
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Release release, CancellationToken ct = default)
        => await db.Releases.AddAsync(release, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
