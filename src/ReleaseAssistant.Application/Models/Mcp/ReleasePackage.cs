namespace ReleaseAssistant.Application.Models.Mcp;

public record ReleasePackage(
    ReleasePackageRelease Release,
    IReadOnlyList<ReleasePackageApplication> Applications,
    IReadOnlyList<ReleasePackageWorkItem> WorkItems,
    IReadOnlyList<ReleasePackagePullRequest> PullRequests,
    IReadOnlyList<ReleasePackageDeployment> Deployments,
    IReadOnlyList<ReleasePackageRollbackCandidate> RollbackCandidates,
    IReadOnlyList<ReleasePackageValidation> Validations);

public record ReleasePackageRelease(
    string ReleaseId,
    string Name,
    string ChangeRequest,
    string Organization,
    string Project,
    string TargetEnvironment,
    string Status);

public record ReleasePackageApplication(
    string ApplicationName,
    string RepositoryName,
    string ProductionEnvironmentName);

public record ReleasePackageWorkItem(
    int Id,
    string Type,
    string Title,
    string State,
    string AssignedTo,
    string Url);

public record ReleasePackagePullRequest(
    int PullRequestId,
    string RepositoryName,
    string Title,
    string Status,
    string TargetBranch,
    string Url);

public record ReleasePackageDeployment(
    string ApplicationName,
    string ReleaseName,
    string EnvironmentName,
    string DeploymentStatus,
    string ApprovalStatus,
    string Url);

public record ReleasePackageRollbackCandidate(
    string ApplicationName,
    string ReleaseName,
    string EnvironmentName,
    string DeploymentStatus,
    string Url);

public record ReleasePackageValidation(
    string Code,
    string Severity,
    string Message);
