using Microsoft.EntityFrameworkCore;
using ReleaseAssistant.Domain.Entities;
using ReleaseAssistant.Domain.Enums;
using ReleaseAssistant.Infrastructure.Data;
using ReleaseAssistant.Infrastructure.Repositories;

namespace ReleaseAssistant.Tests;

public class ReleaseRepositoryWorkItemTests
{
    [Fact]
    public async Task AddWorkItemsAsync_SkipsDuplicateAzureDevOpsWorkItemId()
    {
        await using var db = CreateDb();
        var releaseId = await SeedReleaseAsync(db);
        var repo = new ReleaseRepository(db);

        var first = CreateWorkItem(3, "First title");
        var attached = await repo.AddWorkItemsAsync(releaseId, [first], CancellationToken.None);
        Assert.Equal(1, attached);

        var duplicate = CreateWorkItem(3, "Updated title");
        var reattached = await repo.AddWorkItemsAsync(releaseId, [duplicate], CancellationToken.None);
        Assert.Equal(0, reattached);

        var stored = await db.ReleaseWorkItems
            .Where(w => w.ReleaseId == releaseId)
            .ToListAsync();
        Assert.Single(stored);
        Assert.Equal("Updated title", stored[0].Title);
    }

    [Fact]
    public async Task AddWorkItemsAsync_DeduplicatesWithinBatch()
    {
        await using var db = CreateDb();
        var releaseId = await SeedReleaseAsync(db);
        var repo = new ReleaseRepository(db);

        var attached = await repo.AddWorkItemsAsync(releaseId,
        [
            CreateWorkItem(3, "First"),
            CreateWorkItem(3, "Last wins")
        ], CancellationToken.None);

        Assert.Equal(1, attached);
        var stored = await db.ReleaseWorkItems.Where(w => w.ReleaseId == releaseId).ToListAsync();
        Assert.Single(stored);
        Assert.Equal("Last wins", stored[0].Title);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Guid> SeedReleaseAsync(AppDbContext db)
    {
        var release = new Release
        {
            Name = "Test Release",
            ChangeRequest = "CR-TEST",
            Organization = "org",
            Project = "proj",
            TargetEnvironment = "Production",
            Status = ReleaseStatus.Created
        };
        await db.Releases.AddAsync(release);
        await db.SaveChangesAsync();
        return release.Id;
    }

    private static ReleaseWorkItem CreateWorkItem(int adoId, string title) => new()
    {
        AzureDevOpsWorkItemId = adoId,
        WorkItemType = "Task",
        Title = title,
        State = "Done",
        Url = $"https://dev.azure.com/org/_apis/wit/workItems/{adoId}"
    };
}
