using Microsoft.EntityFrameworkCore;
using ReleaseAssistant.Application.Interfaces;
using ReleaseAssistant.Application.Models.Requests;
using ReleaseAssistant.Application.Models.Responses;
using ReleaseAssistant.Application.Services;
using ReleaseAssistant.Application.Validation;
using ReleaseAssistant.Domain.Entities;
using ReleaseAssistant.Domain.Enums;
using ReleaseAssistant.Infrastructure.Data;
using ReleaseAssistant.Infrastructure.Repositories;

namespace ReleaseAssistant.Tests;

/// <summary>
/// Tests that AnalyzeDeploymentsAsync passes PR merge commits to the collector and
/// emits a DEP004 warning when no commit-matched release is found (fallback by recency).
/// </summary>
public class DeploymentCommitCorrelationTests
{
    // ─── helpers ─────────────────────────────────────────────────────────────

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    private static async Task<(ReleaseService, ReleaseAnalysisService)> BuildServicesAsync(
        AppDbContext db,
        IAzureDevOpsDataCollector collector)
    {
        var releaseRepo = new ReleaseRepository(db);
        var mappingRepo = new ApplicationMappingRepository(db);
        var engine = new ValidationEngine();
        var releaseService = new ReleaseService(releaseRepo, mappingRepo, engine);
        var statusStore = new InMemoryAnalysisStatusStore();
        var analysisService = new ReleaseAnalysisService(releaseService, collector, statusStore);
        return (releaseService, analysisService);
    }

    private static async Task<Guid> CreateReleaseWithPrAsync(
        ReleaseService releaseService,
        AppDbContext db,
        string mergeCommitId)
    {
        var (release, _) = await releaseService.CreateAsync(new CreateReleaseRequest(
            "Test Release", "CR-99", "test-org", "test-project", "Production",
            ["my-app"]));

        // Seed the application mapping so SyncApplicationsFromMappingsAsync finds it.
        db.ApplicationMappings.Add(new ApplicationMapping
        {
            ApplicationName = "my-app",
            Organization = "test-org",
            Project = "test-project",
            RepositoryName = "my-repo",
            ReleaseDefinitionId = 1,
            ProductionEnvironmentName = "Production",
            IsActive = true
        });
        await db.SaveChangesAsync();

        // Attach a PR that carries a known merge commit.
        await releaseService.AttachPullRequestsAsync(new AttachPullRequestsRequest(
            release.Id.ToString(),
            [new PullRequestData(
                PullRequestId: 10,
                RepositoryName: "my-repo",
                Status: "completed",
                TargetBranch: "main",
                MergeCommitId: mergeCommitId)]));

        return release.Id;
    }

    // ─── tests ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task AnalyzeDeployments_Passes_MergeCommits_To_Collector()
    {
        const string expectedCommit = "abc123def456";
        IReadOnlyCollection<string>? capturedCommits = null;

        var fakeCollector = new FakeAdoCollector(onCollectDeployments: (apps, org, proj, commits, ct) =>
        {
            capturedCommits = commits;
            return Task.FromResult<IReadOnlyList<DeploymentData>>([]);
        });

        await using var db = CreateDb();
        var (svc, analysisSvc) = await BuildServicesAsync(db, fakeCollector);
        var releaseId = await CreateReleaseWithPrAsync(svc, db, expectedCommit);

        await analysisSvc.AnalyzeDeploymentsAsync(releaseId);

        Assert.NotNull(capturedCommits);
        Assert.Contains(expectedCommit, capturedCommits);
    }

    [Fact]
    public async Task AnalyzeDeployments_Emits_DEP004_Warning_When_CommitMatched_False()
    {
        var fakeCollector = new FakeAdoCollector(onCollectDeployments: (apps, org, proj, commits, ct) =>
        {
            var dep = new DeploymentData(
                "my-app", "Release-8", "Production", "notStarted",
                "https://example.com/release/8",
                AzureDevOpsReleaseId: 8,
                CommitMatched: false);    // ← fallback by recency, not a commit match
            return Task.FromResult<IReadOnlyList<DeploymentData>>([dep]);
        });

        await using var db = CreateDb();
        var (svc, analysisSvc) = await BuildServicesAsync(db, fakeCollector);
        var releaseId = await CreateReleaseWithPrAsync(svc, db, "abc123");

        var result = await analysisSvc.AnalyzeDeploymentsAsync(releaseId);

        Assert.Contains(result.Warnings, w => w.Contains("DEP004"));
    }

    [Fact]
    public async Task AnalyzeDeployments_No_DEP004_Warning_When_CommitMatched_True()
    {
        var fakeCollector = new FakeAdoCollector(onCollectDeployments: (apps, org, proj, commits, ct) =>
        {
            var dep = new DeploymentData(
                "my-app", "Release-5", "Production", "notStarted",
                "https://example.com/release/5",
                AzureDevOpsReleaseId: 5,
                CommitMatched: true);     // ← exact artifact commit match
            return Task.FromResult<IReadOnlyList<DeploymentData>>([dep]);
        });

        await using var db = CreateDb();
        var (svc, analysisSvc) = await BuildServicesAsync(db, fakeCollector);
        var releaseId = await CreateReleaseWithPrAsync(svc, db, "abc123");

        var result = await analysisSvc.AnalyzeDeploymentsAsync(releaseId);

        Assert.DoesNotContain(result.Warnings, w => w.Contains("DEP004"));
    }

    [Fact]
    public async Task AnalyzeDeployments_No_MergeCommits_Passed_When_PRs_Have_No_Commit()
    {
        IReadOnlyCollection<string>? capturedCommits = null;

        var fakeCollector = new FakeAdoCollector(onCollectDeployments: (apps, org, proj, commits, ct) =>
        {
            capturedCommits = commits;
            return Task.FromResult<IReadOnlyList<DeploymentData>>([]);
        });

        await using var db = CreateDb();
        var (svc, analysisSvc) = await BuildServicesAsync(db, fakeCollector);

        // Attach a PR with no merge commit.
        var (release, _) = await svc.CreateAsync(new CreateReleaseRequest(
            "No-Commit Release", "CR-100", "org", "proj", "Production", ["my-app"]));
        db.ApplicationMappings.Add(new ApplicationMapping
        {
            ApplicationName = "my-app",
            Organization = "org",
            Project = "proj",
            RepositoryName = "my-repo",
            ReleaseDefinitionId = 1,
            ProductionEnvironmentName = "Production",
            IsActive = true
        });
        await db.SaveChangesAsync();
        await svc.AttachPullRequestsAsync(new AttachPullRequestsRequest(
            release.Id.ToString(),
            [new PullRequestData(PullRequestId: 11, RepositoryName: "my-repo",
                Status: "completed", TargetBranch: "main", MergeCommitId: null)]));

        await analysisSvc.AnalyzeDeploymentsAsync(release.Id);

        // The commit list should be empty (not null, but contains nothing).
        Assert.NotNull(capturedCommits);
        Assert.Empty(capturedCommits);
    }

    [Fact]
    public async Task AnalyzeDeployments_Uses_WorkItem_Linked_Deployment_When_Available()
    {
        // When work items have a deployment link, that result should be used and CommitMatched is true.
        var linkedDep = new DeploymentData(
            "my-app", "Release-10", "Production", "notStarted",
            "https://example.com/release/10",
            AzureDevOpsReleaseId: 10,
            CommitMatched: true);

        var fakeCollector = new FakeAdoCollector(
            onCollectFromWorkItemLinks: (ids, apps, org, proj, ct) =>
                Task.FromResult<IReadOnlyList<DeploymentData>>([linkedDep]));

        await using var db = CreateDb();
        var (svc, analysisSvc) = await BuildServicesAsync(db, fakeCollector);
        var releaseId = await CreateReleaseWithPrAsync(svc, db, "abc123");

        var result = await analysisSvc.AnalyzeDeploymentsAsync(releaseId);

        Assert.Equal(1, result.AttachedCount);
        Assert.DoesNotContain(result.Warnings, w => w.Contains("DEP004"));
    }

    [Fact]
    public async Task AnalyzeDeployments_WorkItemLinks_Take_Priority_Fallback_Only_For_Uncovered_Apps()
    {
        // App "my-app" is covered by the work-item link; "other-app" should fall back.
        var linkedDep = new DeploymentData(
            "my-app", "Release-10", "Production", "notStarted",
            "https://example.com/release/10", AzureDevOpsReleaseId: 10, CommitMatched: true);
        var fallbackDep = new DeploymentData(
            "other-app", "Release-3", "Production", "succeeded",
            "https://example.com/release/3", AzureDevOpsReleaseId: 3, CommitMatched: false);

        IReadOnlyList<(string, int, string)>? fallbackApps = null;

        var fakeCollector = new FakeAdoCollector(
            onCollectDeployments: (apps, org, proj, commits, ct) =>
            {
                fallbackApps = apps;
                return Task.FromResult<IReadOnlyList<DeploymentData>>([fallbackDep]);
            },
            onCollectFromWorkItemLinks: (ids, apps, org, proj, ct) =>
                Task.FromResult<IReadOnlyList<DeploymentData>>([linkedDep]));

        await using var db = CreateDb();
        var (svc, analysisSvc) = await BuildServicesAsync(db, fakeCollector);
        var releaseId = await CreateReleaseWithPrAsync(svc, db, "abc123");

        var result = await analysisSvc.AnalyzeDeploymentsAsync(releaseId);

        // Only "other-app" was passed to the fallback collector.
        Assert.NotNull(fallbackApps);
        Assert.DoesNotContain(fallbackApps, a => a.Item1 == "my-app");
        // DEP004 is emitted for the fallback result, not for the linked one.
        Assert.Contains(result.Warnings, w => w.Contains("DEP004") && w.Contains("other-app"));
        Assert.DoesNotContain(result.Warnings, w => w.Contains("DEP004") && w.Contains("my-app"));
    }

    [Fact]
    public async Task AnalyzeDeployments_Falls_Back_To_CurrentDeployments_When_No_WorkItem_Links()
    {
        // When work-item links return nothing, the old commit-based strategy is used.
        const string expectedCommit = "def789";
        IReadOnlyCollection<string>? capturedCommits = null;

        var fakeCollector = new FakeAdoCollector(
            onCollectDeployments: (apps, org, proj, commits, ct) =>
            {
                capturedCommits = commits;
                return Task.FromResult<IReadOnlyList<DeploymentData>>([]);
            });
        // onCollectFromWorkItemLinks not supplied → returns empty list.

        await using var db = CreateDb();
        var (svc, analysisSvc) = await BuildServicesAsync(db, fakeCollector);
        var releaseId = await CreateReleaseWithPrAsync(svc, db, expectedCommit);

        await analysisSvc.AnalyzeDeploymentsAsync(releaseId);

        Assert.NotNull(capturedCommits);
        Assert.Contains(expectedCommit, capturedCommits);
    }
}

// ─── Fake IAzureDevOpsDataCollector ──────────────────────────────────────────

file sealed class FakeAdoCollector(
    Func<
        IReadOnlyList<(string, int, string)>,
        string, string,
        IReadOnlyCollection<string>?,
        CancellationToken,
        Task<IReadOnlyList<DeploymentData>>>? onCollectDeployments = null,
    Func<
        IReadOnlyList<int>,
        IReadOnlyList<(string, int, string)>,
        string, string,
        CancellationToken,
        Task<IReadOnlyList<DeploymentData>>>? onCollectFromWorkItemLinks = null) : IAzureDevOpsDataCollector
{
    public Task<IReadOnlyList<WorkItemData>> CollectWorkItemsByTagAsync(
        string tag, string organization, string project, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<WorkItemData>>([]);

    public Task<IReadOnlyList<PullRequestData>> CollectPullRequestsForWorkItemsAsync(
        IReadOnlyList<WorkItemData> workItems, string organization, string project,
        CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<PullRequestData>>([]);

    public Task<IReadOnlyList<DeploymentData>> CollectCurrentDeploymentsAsync(
        IReadOnlyList<(string ApplicationName, int ReleaseDefinitionId, string EnvironmentName)> applications,
        string organization, string project,
        IReadOnlyCollection<string>? mergeCommits = null,
        CancellationToken ct = default)
        => onCollectDeployments != null
            ? onCollectDeployments(applications, organization, project, mergeCommits, ct)
            : Task.FromResult<IReadOnlyList<DeploymentData>>([]);

    public Task<IReadOnlyList<DeploymentData>> CollectDeploymentsFromWorkItemLinksAsync(
        IReadOnlyList<int> workItemIds,
        IReadOnlyList<(string ApplicationName, int ReleaseDefinitionId, string EnvironmentName)> applications,
        string organization, string project,
        CancellationToken ct = default)
        => onCollectFromWorkItemLinks != null
            ? onCollectFromWorkItemLinks(workItemIds, applications, organization, project, ct)
            : Task.FromResult<IReadOnlyList<DeploymentData>>([]);

    public Task<IReadOnlyList<RollbackCandidateData>> CollectRollbackCandidatesAsync(
        IReadOnlyList<(string ApplicationName, int ReleaseDefinitionId, string EnvironmentName, int? CurrentReleaseId)> applications,
        string organization, string project, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<RollbackCandidateData>>([]);
}
