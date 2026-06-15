using ReleaseAssistant.Domain.Entities;

namespace ReleaseAssistant.Application.Models.Responses;

public static class ReleaseDetailMapper
{
    public static ReleaseDetailResponse ToDetailResponse(Release release) =>
        new(
            release.Id,
            release.Name,
            release.ChangeRequest,
            release.Organization,
            release.Project,
            release.TargetEnvironment,
            release.Status.ToString(),
            release.CreatedBy,
            release.CreatedAt,
            release.UpdatedAt,
            release.Applications.Select(a => new ReleaseApplicationDetail(
                a.Id, a.ApplicationName, a.RepositoryName, a.BuildDefinitionId,
                a.ReleaseDefinitionId, a.ProductionEnvironmentName, a.UatEnvironmentName)).ToList(),
            release.WorkItems.Select(w => new ReleaseWorkItemDetail(
                w.Id, w.AzureDevOpsWorkItemId, w.WorkItemType, w.Title, w.State, w.AssignedTo, w.Url)).ToList(),
            release.PullRequests.Select(p => new ReleasePullRequestDetail(
                p.Id, p.AzureDevOpsPullRequestId, p.RepositoryName, p.Title, p.Status,
                p.SourceBranch, p.TargetBranch, p.Url)).ToList(),
            release.Deployments.Select(d => new ReleaseDeploymentDetail(
                d.Id, d.ApplicationName, d.AzureDevOpsReleaseName, d.AzureDevOpsReleaseId,
                d.EnvironmentName, d.DeploymentStatus, d.ApprovalStatus, d.DeploymentUrl,
                d.IsCurrentDeployment)).ToList(),
            release.RollbackCandidates.Select(r => new ReleaseRollbackCandidateDetail(
                r.Id, r.ApplicationName, r.AzureDevOpsReleaseName, r.AzureDevOpsReleaseId,
                r.EnvironmentName, r.DeploymentStatus, r.RollbackUrl)).ToList(),
            release.ValidationResults.Select(v => new ReleaseValidationResultDetail(
                v.RuleCode, v.Severity.ToString(), v.Status.ToString(), v.Message)).ToList(),
            release.Documents.Select(d => new ReleaseDocumentDetail(
                d.Id, d.Version, d.Format, d.GeneratedAt)).ToList());
}
