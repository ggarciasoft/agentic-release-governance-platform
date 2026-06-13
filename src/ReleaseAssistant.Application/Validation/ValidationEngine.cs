using ReleaseAssistant.Application.Interfaces;
using ReleaseAssistant.Application.Models.Responses;
using ReleaseAssistant.Domain.Entities;
using ReleaseAssistant.Domain.Enums;

namespace ReleaseAssistant.Application.Validation;

public class ValidationEngine : IValidationEngine
{
    private static readonly HashSet<string> ReadyStates = new(StringComparer.OrdinalIgnoreCase)
    {
        "UATDone", "Ready for Release", "Approved for Production", "Closed", "Resolved", "Done"
    };

    private static readonly HashSet<string> WarnStates = new(StringComparer.OrdinalIgnoreCase)
    {
        "UATReady", "In Testing", "QA Ready", "Ready for UAT"
    };

    private static readonly HashSet<string> BlockStates = new(StringComparer.OrdinalIgnoreCase)
    {
        "New", "Active", "In Progress", "Rejected", "Removed", "Blocked"
    };

    private static readonly HashSet<string> AllowedBranches = new(StringComparer.OrdinalIgnoreCase)
    {
        "main", "master"
    };

    private static readonly HashSet<string> ValidDeploymentStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Queued", "PendingApproval", "Waiting", "InProgress", "Ready"
    };

    public Task<ValidationSummary> ValidateAsync(Release release, CancellationToken ct = default)
    {
        var findings = new List<ValidationFinding>();

        if (release.WorkItems.Count == 0 && release.Deployments.Count == 0)
        {
            return Task.FromResult(new ValidationSummary(
                ReadinessStatus.Incomplete,
                Array.Empty<ValidationFinding>(),
                Array.Empty<ValidationFinding>(),
                Array.Empty<ValidationFinding>()));
        }

        ValidateWorkItems(release, findings);
        ValidatePullRequests(release, findings);
        ValidateApplications(release, findings);
        ValidateDeployments(release, findings);
        ValidateRollbackCandidates(release, findings);

        var status = CalculateStatus(findings);

        return Task.FromResult(new ValidationSummary(
            status,
            findings.Where(f => f.Severity is FindingSeverity.Blocker or FindingSeverity.Critical).ToList(),
            findings.Where(f => f.Severity == FindingSeverity.Warning).ToList(),
            findings.Where(f => f.Severity == FindingSeverity.Info).ToList()));
    }

    private static void ValidateWorkItems(Release release, List<ValidationFinding> findings)
    {
        foreach (var wi in release.WorkItems)
        {
            if (wi.WorkItemType.Equals("Bug", StringComparison.OrdinalIgnoreCase)
                && !ReadyStates.Contains(wi.State))
            {
                findings.Add(new ValidationFinding(
                    "WI004", FindingSeverity.Critical,
                    $"Critical bug {wi.AzureDevOpsWorkItemId} '{wi.Title}' is in state '{wi.State}' and must be closed or waived.",
                    "WorkItem", wi.AzureDevOpsWorkItemId.ToString()));
                continue;
            }

            if (BlockStates.Contains(wi.State))
            {
                findings.Add(new ValidationFinding(
                    "WI003", FindingSeverity.Blocker,
                    $"Work item {wi.AzureDevOpsWorkItemId} '{wi.Title}' is in blocking state '{wi.State}'.",
                    "WorkItem", wi.AzureDevOpsWorkItemId.ToString()));
            }
            else if (WarnStates.Contains(wi.State))
            {
                findings.Add(new ValidationFinding(
                    "WI002", FindingSeverity.Warning,
                    $"Work item {wi.AzureDevOpsWorkItemId} '{wi.Title}' is in warning state '{wi.State}' (not yet UATDone).",
                    "WorkItem", wi.AzureDevOpsWorkItemId.ToString()));
            }
            else if (!ReadyStates.Contains(wi.State))
            {
                findings.Add(new ValidationFinding(
                    "WI001", FindingSeverity.Warning,
                    $"Work item {wi.AzureDevOpsWorkItemId} '{wi.Title}' is in unknown state '{wi.State}'.",
                    "WorkItem", wi.AzureDevOpsWorkItemId.ToString()));
            }
        }
    }

    private static void ValidatePullRequests(Release release, List<ValidationFinding> findings)
    {
        foreach (var pr in release.PullRequests)
        {
            if (pr.Status.Equals("abandoned", StringComparison.OrdinalIgnoreCase))
            {
                findings.Add(new ValidationFinding(
                    "PR003", FindingSeverity.Blocker,
                    $"PR #{pr.AzureDevOpsPullRequestId} '{pr.Title}' is abandoned.",
                    "PullRequest", pr.AzureDevOpsPullRequestId.ToString()));
                continue;
            }

            if (pr.Status.Equals("active", StringComparison.OrdinalIgnoreCase))
            {
                findings.Add(new ValidationFinding(
                    "PR004", FindingSeverity.Blocker,
                    $"PR #{pr.AzureDevOpsPullRequestId} '{pr.Title}' is still active (not merged).",
                    "PullRequest", pr.AzureDevOpsPullRequestId.ToString()));
                continue;
            }

            if (!pr.Status.Equals("completed", StringComparison.OrdinalIgnoreCase))
            {
                findings.Add(new ValidationFinding(
                    "PR001", FindingSeverity.Warning,
                    $"PR #{pr.AzureDevOpsPullRequestId} '{pr.Title}' has unexpected status '{pr.Status}'.",
                    "PullRequest", pr.AzureDevOpsPullRequestId.ToString()));
            }

            var targetBranch = pr.TargetBranch.Replace("refs/heads/", "");
            var isAllowed = AllowedBranches.Contains(targetBranch)
                || targetBranch.StartsWith("release/", StringComparison.OrdinalIgnoreCase);
            if (!isAllowed)
            {
                findings.Add(new ValidationFinding(
                    "PR002", FindingSeverity.Warning,
                    $"PR #{pr.AzureDevOpsPullRequestId} targets branch '{targetBranch}' which is not in the allowed list.",
                    "PullRequest", pr.AzureDevOpsPullRequestId.ToString()));
            }
        }
    }

    private static void ValidateApplications(Release release, List<ValidationFinding> findings)
    {
        foreach (var app in release.Applications)
        {
            var hasMapping = app.ReleaseDefinitionId.HasValue
                || app.BuildDefinitionId.HasValue
                || !string.IsNullOrWhiteSpace(app.RepositoryName);

            if (!hasMapping)
            {
                findings.Add(new ValidationFinding(
                    "APP001", FindingSeverity.Warning,
                    $"Application '{app.ApplicationName}' has no configured pipeline or repository mapping. Add an ApplicationMapping to resolve pipelines.",
                    "Application", app.ApplicationName));
            }
        }
    }

    private static void ValidateDeployments(Release release, List<ValidationFinding> findings)
    {
        foreach (var app in release.Applications)
        {
            var deployment = release.Deployments
                .FirstOrDefault(d => d.ApplicationName.Equals(app.ApplicationName, StringComparison.OrdinalIgnoreCase)
                    && d.IsCurrentDeployment);

            if (deployment == null)
            {
                findings.Add(new ValidationFinding(
                    "DEP001", FindingSeverity.Blocker,
                    $"Application '{app.ApplicationName}' has no production deployment candidate.",
                    "Application", app.ApplicationName));
                continue;
            }

            if (string.IsNullOrWhiteSpace(deployment.DeploymentUrl))
            {
                findings.Add(new ValidationFinding(
                    "DEP002", FindingSeverity.Warning,
                    $"Deployment candidate for '{app.ApplicationName}' has no URL.",
                    "Deployment", deployment.Id.ToString()));
            }

            if (!ValidDeploymentStatuses.Contains(deployment.DeploymentStatus))
            {
                findings.Add(new ValidationFinding(
                    "DEP003", FindingSeverity.Warning,
                    $"Deployment for '{app.ApplicationName}' has unexpected status '{deployment.DeploymentStatus}'.",
                    "Deployment", deployment.Id.ToString()));
            }

            if (deployment.ApprovalStatus.Equals("rejected", StringComparison.OrdinalIgnoreCase))
            {
                findings.Add(new ValidationFinding(
                    "AP002", FindingSeverity.Blocker,
                    $"Production approval for '{app.ApplicationName}' has been rejected.",
                    "Deployment", deployment.Id.ToString()));
            }
            else if (deployment.ApprovalStatus.Equals("pending", StringComparison.OrdinalIgnoreCase))
            {
                findings.Add(new ValidationFinding(
                    "AP003", FindingSeverity.Warning,
                    $"Production approval for '{app.ApplicationName}' is still pending.",
                    "Deployment", deployment.Id.ToString()));
            }
        }
    }

    private static void ValidateRollbackCandidates(Release release, List<ValidationFinding> findings)
    {
        foreach (var app in release.Applications)
        {
            var rollback = release.RollbackCandidates
                .FirstOrDefault(r => r.ApplicationName.Equals(app.ApplicationName, StringComparison.OrdinalIgnoreCase));

            if (rollback == null)
            {
                findings.Add(new ValidationFinding(
                    "RB001", FindingSeverity.Warning,
                    $"Application '{app.ApplicationName}' has no rollback candidate.",
                    "Application", app.ApplicationName));
                continue;
            }

            if (!rollback.DeploymentStatus.Equals("Succeeded", StringComparison.OrdinalIgnoreCase))
            {
                findings.Add(new ValidationFinding(
                    "RB002", FindingSeverity.Warning,
                    $"Rollback candidate for '{app.ApplicationName}' did not succeed (status: {rollback.DeploymentStatus}).",
                    "RollbackCandidate", rollback.Id.ToString()));
            }

            if (string.IsNullOrWhiteSpace(rollback.RollbackUrl))
            {
                findings.Add(new ValidationFinding(
                    "RB001", FindingSeverity.Warning,
                    $"Rollback candidate for '{app.ApplicationName}' has no URL.",
                    "RollbackCandidate", rollback.Id.ToString()));
            }
        }
    }

    private static ReadinessStatus CalculateStatus(List<ValidationFinding> findings)
    {
        if (findings.Any(f => f.Severity is FindingSeverity.Critical or FindingSeverity.Blocker))
            return ReadinessStatus.Blocked;
        if (findings.Any(f => f.Severity == FindingSeverity.Warning))
            return ReadinessStatus.Warning;
        return ReadinessStatus.Ready;
    }
}
