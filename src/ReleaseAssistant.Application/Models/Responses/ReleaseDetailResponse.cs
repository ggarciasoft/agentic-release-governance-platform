namespace ReleaseAssistant.Application.Models.Responses;

public record ReleaseDetailResponse(
    Guid Id,
    string Name,
    string ChangeRequest,
    string Organization,
    string Project,
    string TargetEnvironment,
    string Status,
    string CreatedBy,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<ReleaseApplicationDetail> Applications,
    IReadOnlyList<ReleaseWorkItemDetail> WorkItems,
    IReadOnlyList<ReleasePullRequestDetail> PullRequests,
    IReadOnlyList<ReleaseDeploymentDetail> Deployments,
    IReadOnlyList<ReleaseRollbackCandidateDetail> RollbackCandidates,
    IReadOnlyList<ReleaseValidationResultDetail> ValidationResults,
    IReadOnlyList<ReleaseDocumentDetail> Documents);

public record ReleaseApplicationDetail(
    Guid Id,
    string ApplicationName,
    string RepositoryName,
    int? BuildDefinitionId,
    int? ReleaseDefinitionId,
    string ProductionEnvironmentName,
    string UatEnvironmentName);

public record ReleaseWorkItemDetail(
    Guid Id,
    int AzureDevOpsWorkItemId,
    string WorkItemType,
    string Title,
    string State,
    string AssignedTo,
    string Url);

public record ReleasePullRequestDetail(
    Guid Id,
    int AzureDevOpsPullRequestId,
    string RepositoryName,
    string Title,
    string Status,
    string SourceBranch,
    string TargetBranch,
    string Url);

public record ReleaseDeploymentDetail(
    Guid Id,
    string ApplicationName,
    string AzureDevOpsReleaseName,
    int? AzureDevOpsReleaseId,
    string EnvironmentName,
    string DeploymentStatus,
    string ApprovalStatus,
    string DeploymentUrl,
    bool IsCurrentDeployment);

public record ReleaseRollbackCandidateDetail(
    Guid Id,
    string ApplicationName,
    string AzureDevOpsReleaseName,
    int? AzureDevOpsReleaseId,
    string EnvironmentName,
    string DeploymentStatus,
    string RollbackUrl);

public record ReleaseValidationResultDetail(
    string RuleCode,
    string Severity,
    string Status,
    string Message);

public record ReleaseDocumentDetail(
    Guid Id,
    int Version,
    string Format,
    DateTime GeneratedAt);
