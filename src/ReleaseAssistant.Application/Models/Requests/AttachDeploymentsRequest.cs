namespace ReleaseAssistant.Application.Models.Requests;

public record DeploymentData(
    string ApplicationName,
    string ReleaseName,
    string EnvironmentName,
    string Status,
    string Url,
    int? AzureDevOpsReleaseId = null,
    int? ReleaseDefinitionId = null,
    string ReleaseDefinitionName = "",
    string ApprovalStatus = "",
    DateTime? StartedAt = null,
    DateTime? CompletedAt = null,
    string RawJson = "{}");

public record AttachDeploymentsRequest(
    string ReleaseId,
    IReadOnlyList<DeploymentData> Deployments);
