using System.Text;
using ReleaseAssistant.Application.Interfaces;
using ReleaseAssistant.Application.Models.Mcp;

namespace ReleaseAssistant.Agents.Documents;

public class MarkdownDocumentGenerator : IDocumentGenerator
{
    public string Generate(ReleasePackage package)
    {
        var sb = new StringBuilder();
        var r = package.Release;

        sb.AppendLine($"# Production Release Document");
        sb.AppendLine();
        sb.AppendLine($"**Generated:** {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine();

        // Release Summary
        sb.AppendLine("## Release Summary");
        sb.AppendLine();
        sb.AppendLine($"| Field | Value |");
        sb.AppendLine($"|---|---|");
        sb.AppendLine($"| Release Name | {r.Name} |");
        sb.AppendLine($"| Change Request | {r.ChangeRequest} |");
        sb.AppendLine($"| Organization | {r.Organization} |");
        sb.AppendLine($"| Project | {r.Project} |");
        sb.AppendLine($"| Target Environment | {r.TargetEnvironment} |");
        sb.AppendLine($"| Status | {r.Status} |");
        sb.AppendLine($"| Release ID | {r.ReleaseId} |");
        sb.AppendLine();

        // Validation Summary
        var blockers = package.Validations.Where(v => v.Severity is "Blocker" or "Critical").ToList();
        var warnings = package.Validations.Where(v => v.Severity == "Warning").ToList();

        sb.AppendLine("## Validation Summary");
        sb.AppendLine();
        if (!package.Validations.Any())
        {
            sb.AppendLine("> Validation has not been run for this release.");
        }
        else
        {
            var overallStatus = blockers.Any() ? "🔴 **Blocked**"
                : warnings.Any() ? "🟡 **Warning**"
                : "🟢 **Ready**";
            sb.AppendLine($"**Overall Status:** {overallStatus}");
            sb.AppendLine();
        }
        sb.AppendLine();

        if (blockers.Any())
        {
            sb.AppendLine("## Blockers");
            sb.AppendLine();
            foreach (var b in blockers)
                sb.AppendLine($"- ❌ `{b.Code}` {b.Message}");
            sb.AppendLine();
        }

        if (warnings.Any())
        {
            sb.AppendLine("## Warnings");
            sb.AppendLine();
            foreach (var w in warnings)
                sb.AppendLine($"- ⚠️ `{w.Code}` {w.Message}");
            sb.AppendLine();
        }

        // Applications
        sb.AppendLine("## Applications Included");
        sb.AppendLine();
        if (!package.Applications.Any())
        {
            sb.AppendLine("_Missing_");
        }
        else
        {
            sb.AppendLine("| Application | Repository | Environment |");
            sb.AppendLine("|---|---|---|");
            foreach (var a in package.Applications)
                sb.AppendLine($"| {a.ApplicationName} | {NullOrValue(a.RepositoryName)} | {a.ProductionEnvironmentName} |");
        }
        sb.AppendLine();

        // Work Items
        sb.AppendLine("## Work Items Included");
        sb.AppendLine();
        if (!package.WorkItems.Any())
        {
            sb.AppendLine("_No work items attached to this release._");
        }
        else
        {
            sb.AppendLine("| ID | Type | Title | State | Assigned To |");
            sb.AppendLine("|---|---|---|---|---|");
            foreach (var wi in package.WorkItems)
                sb.AppendLine($"| [{wi.Id}]({wi.Url}) | {wi.Type} | {wi.Title} | {wi.State} | {NullOrValue(wi.AssignedTo)} |");
        }
        sb.AppendLine();

        // Pull Requests
        sb.AppendLine("## Pull Requests Included");
        sb.AppendLine();
        if (!package.PullRequests.Any())
        {
            sb.AppendLine("_No pull requests attached to this release._");
        }
        else
        {
            sb.AppendLine("| PR # | Repository | Title | Status | Target Branch |");
            sb.AppendLine("|---|---|---|---|---|");
            foreach (var pr in package.PullRequests)
                sb.AppendLine($"| [{pr.PullRequestId}]({pr.Url}) | {pr.RepositoryName} | {pr.Title} | {pr.Status} | {pr.TargetBranch} |");
        }
        sb.AppendLine();

        // Deployments
        sb.AppendLine("## Deployment Information");
        sb.AppendLine();
        if (!package.Deployments.Any())
        {
            sb.AppendLine("_No deployment candidates found. ⚠️_");
        }
        else
        {
            sb.AppendLine("| Application | Release Name | Environment | Status | Approval | Link |");
            sb.AppendLine("|---|---|---|---|---|---|");
            foreach (var d in package.Deployments)
                sb.AppendLine($"| {d.ApplicationName} | {d.ReleaseName} | {d.EnvironmentName} | {d.DeploymentStatus} | {NullOrValue(d.ApprovalStatus)} | {LinkOrMissing(d.Url)} |");
        }
        sb.AppendLine();

        // Rollback
        sb.AppendLine("## Rollback Plan");
        sb.AppendLine();
        if (!package.RollbackCandidates.Any())
        {
            sb.AppendLine("_No rollback candidates found. ⚠️_");
        }
        else
        {
            sb.AppendLine("| Application | Rollback Release | Environment | Status | Link |");
            sb.AppendLine("|---|---|---|---|---|");
            foreach (var rc in package.RollbackCandidates)
                sb.AppendLine($"| {rc.ApplicationName} | {rc.ReleaseName} | {rc.EnvironmentName} | {rc.DeploymentStatus} | {LinkOrMissing(rc.Url)} |");
        }
        sb.AppendLine();

        // Post-Deployment Checklist
        sb.AppendLine("## Post-Deployment Checklist");
        sb.AppendLine();
        sb.AppendLine("- [ ] Verify all application health checks are green.");
        sb.AppendLine("- [ ] Confirm deployment completed successfully in Azure DevOps.");
        sb.AppendLine("- [ ] Notify stakeholders of deployment completion.");
        sb.AppendLine("- [ ] Monitor error rates and response times for 30 minutes.");
        sb.AppendLine("- [ ] Document any issues encountered during deployment.");
        sb.AppendLine();

        // Approval Notes
        sb.AppendLine("## Approval Notes");
        sb.AppendLine();
        sb.AppendLine("_This release document must be reviewed and approved by the Release Manager before proceeding to production._");
        sb.AppendLine();

        return sb.ToString();
    }

    private static string NullOrValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? "_—_" : value;

    private static string LinkOrMissing(string? url)
        => string.IsNullOrWhiteSpace(url) ? "_Missing_" : $"[Link]({url})";
}
