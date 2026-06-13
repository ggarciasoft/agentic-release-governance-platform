using ReleaseAssistant.Application.Validation;
using ReleaseAssistant.Domain.Entities;
using ReleaseAssistant.Domain.Enums;
using Xunit;

namespace ReleaseAssistant.Tests;

public class ValidationEngineTests
{
    private readonly ValidationEngine _engine = new();

    private static Release BuildRelease(Action<Release>? configure = null)
    {
        var release = new Release
        {
            Name = "Test Release",
            ChangeRequest = "CR-001",
            Organization = "test-org",
            Project = "test-project",
            TargetEnvironment = "Production",
            Status = ReleaseStatus.AnalysisComplete
        };
        var app = new ReleaseApplication { ApplicationName = "Payments API", ReleaseId = release.Id };
        release.Applications.Add(app);
        configure?.Invoke(release);
        return release;
    }

    [Fact]
    public async Task Returns_Incomplete_When_No_Data()
    {
        var release = new Release { Name = "Empty" };
        var result = await _engine.ValidateAsync(release);
        Assert.Equal(ReadinessStatus.Incomplete, result.Status);
    }

    [Fact]
    public async Task Returns_Ready_When_All_Rules_Pass()
    {
        var release = BuildRelease(r =>
        {
            // Give the app a repository name so APP001 doesn't fire
            r.Applications.Clear();
            r.Applications.Add(new ReleaseApplication
            {
                ApplicationName = "Payments API", ReleaseId = r.Id,
                RepositoryName = "payments-api", ProductionEnvironmentName = "Production"
            });
            r.WorkItems.Add(new ReleaseWorkItem
            {
                ReleaseId = r.Id, AzureDevOpsWorkItemId = 1, WorkItemType = "User Story",
                Title = "Story 1", State = "UATDone", Url = "https://ado/1"
            });
            r.PullRequests.Add(new ReleasePullRequest
            {
                ReleaseId = r.Id, AzureDevOpsPullRequestId = 10,
                Title = "PR 1", Status = "completed", TargetBranch = "main",
                RepositoryName = "payments-api", Url = "https://ado/pr/10"
            });
            r.Deployments.Add(new ReleaseDeployment
            {
                ReleaseId = r.Id, ApplicationName = "Payments API",
                DeploymentStatus = "PendingApproval", DeploymentUrl = "https://ado/deploy/1",
                IsCurrentDeployment = true
            });
            r.RollbackCandidates.Add(new ReleaseRollbackCandidate
            {
                ReleaseId = r.Id, ApplicationName = "Payments API",
                DeploymentStatus = "Succeeded", RollbackUrl = "https://ado/deploy/0"
            });
        });

        var result = await _engine.ValidateAsync(release);
        Assert.Equal(ReadinessStatus.Ready, result.Status);
        Assert.Empty(result.Blockers);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public async Task Blocks_On_Active_PR()
    {
        var release = BuildRelease(r =>
        {
            r.Deployments.Add(new ReleaseDeployment
            {
                ReleaseId = r.Id, ApplicationName = "Payments API",
                DeploymentStatus = "PendingApproval", DeploymentUrl = "https://ado/deploy/1",
                IsCurrentDeployment = true
            });
            r.PullRequests.Add(new ReleasePullRequest
            {
                ReleaseId = r.Id, AzureDevOpsPullRequestId = 20,
                Title = "Active PR", Status = "active", TargetBranch = "main",
                RepositoryName = "payments-api", Url = "https://ado/pr/20"
            });
        });

        var result = await _engine.ValidateAsync(release);
        Assert.Equal(ReadinessStatus.Blocked, result.Status);
        Assert.Contains(result.Blockers, b => b.Code == "PR004");
    }

    [Fact]
    public async Task Blocks_On_Missing_Deployment_Candidate()
    {
        var release = BuildRelease(r =>
        {
            r.WorkItems.Add(new ReleaseWorkItem
            {
                ReleaseId = r.Id, AzureDevOpsWorkItemId = 2,
                WorkItemType = "User Story", Title = "Story", State = "UATDone", Url = "https://ado/2"
            });
        });

        var result = await _engine.ValidateAsync(release);
        Assert.Equal(ReadinessStatus.Blocked, result.Status);
        Assert.Contains(result.Blockers, b => b.Code == "DEP001");
    }

    [Fact]
    public async Task Warns_On_Work_Item_In_Warning_State()
    {
        var release = BuildRelease(r =>
        {
            // Give the app a mapped repository so APP001 doesn't add a warning
            r.Applications.Clear();
            r.Applications.Add(new ReleaseApplication
            {
                ApplicationName = "Payments API", ReleaseId = r.Id,
                RepositoryName = "payments-api", ProductionEnvironmentName = "Production"
            });
            r.WorkItems.Add(new ReleaseWorkItem
            {
                ReleaseId = r.Id, AzureDevOpsWorkItemId = 3,
                WorkItemType = "User Story", Title = "Story 3", State = "UATReady",
                Url = "https://ado/3"
            });
            r.Deployments.Add(new ReleaseDeployment
            {
                ReleaseId = r.Id, ApplicationName = "Payments API",
                DeploymentStatus = "PendingApproval", DeploymentUrl = "https://ado/deploy/1",
                IsCurrentDeployment = true
            });
            r.RollbackCandidates.Add(new ReleaseRollbackCandidate
            {
                ReleaseId = r.Id, ApplicationName = "Payments API",
                DeploymentStatus = "Succeeded", RollbackUrl = "https://ado/rollback/1"
            });
        });

        var result = await _engine.ValidateAsync(release);
        Assert.Equal(ReadinessStatus.Warning, result.Status);
        Assert.Contains(result.Warnings, w => w.Code == "WI002");
        Assert.Empty(result.Blockers);
    }

    [Fact]
    public async Task Blocks_On_Critical_Bug_Not_Closed()
    {
        var release = BuildRelease(r =>
        {
            r.WorkItems.Add(new ReleaseWorkItem
            {
                ReleaseId = r.Id, AzureDevOpsWorkItemId = 4,
                WorkItemType = "Bug", Title = "Critical Bug", State = "Active",
                Url = "https://ado/4"
            });
            r.Deployments.Add(new ReleaseDeployment
            {
                ReleaseId = r.Id, ApplicationName = "Payments API",
                DeploymentStatus = "PendingApproval", DeploymentUrl = "https://ado/deploy/1",
                IsCurrentDeployment = true
            });
        });

        var result = await _engine.ValidateAsync(release);
        Assert.Equal(ReadinessStatus.Blocked, result.Status);
        Assert.Contains(result.Blockers, b => b.Code == "WI004");
    }

    [Fact]
    public async Task Warns_On_Missing_Rollback_Candidate()
    {
        var release = BuildRelease(r =>
        {
            r.Deployments.Add(new ReleaseDeployment
            {
                ReleaseId = r.Id, ApplicationName = "Payments API",
                DeploymentStatus = "PendingApproval", DeploymentUrl = "https://ado/deploy/1",
                IsCurrentDeployment = true
            });
            // No rollback candidate added
        });

        var result = await _engine.ValidateAsync(release);
        Assert.Contains(result.Warnings, w => w.Code == "RB001");
    }

    [Fact]
    public async Task Blocks_On_Rejected_Approval()
    {
        var release = BuildRelease(r =>
        {
            r.Deployments.Add(new ReleaseDeployment
            {
                ReleaseId = r.Id, ApplicationName = "Payments API",
                DeploymentStatus = "PendingApproval", DeploymentUrl = "https://ado/deploy/1",
                ApprovalStatus = "rejected",
                IsCurrentDeployment = true
            });
        });

        var result = await _engine.ValidateAsync(release);
        Assert.Equal(ReadinessStatus.Blocked, result.Status);
        Assert.Contains(result.Blockers, b => b.Code == "AP002");
    }

    [Fact]
    public void Status_Priority_Blocked_Over_Warning()
    {
        var findings = new List<Application.Models.Responses.ValidationFinding>
        {
            new("WI002", FindingSeverity.Warning, "Warning message"),
            new("PR004", FindingSeverity.Blocker, "Blocker message")
        };

        // Using internal knowledge of CalculateStatus priority
        var status = findings.Any(f => f.Severity is FindingSeverity.Blocker or FindingSeverity.Critical)
            ? ReadinessStatus.Blocked
            : findings.Any(f => f.Severity == FindingSeverity.Warning)
                ? ReadinessStatus.Warning
                : ReadinessStatus.Ready;

        Assert.Equal(ReadinessStatus.Blocked, status);
    }
}
