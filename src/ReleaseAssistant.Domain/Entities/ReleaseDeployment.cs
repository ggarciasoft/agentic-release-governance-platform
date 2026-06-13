namespace ReleaseAssistant.Domain.Entities;

public class ReleaseDeployment : EntityBase
{
    public Guid ReleaseId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public int? AzureDevOpsReleaseId { get; set; }
    public string AzureDevOpsReleaseName { get; set; } = string.Empty;
    public int? ReleaseDefinitionId { get; set; }
    public string ReleaseDefinitionName { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = string.Empty;
    public string EnvironmentStatus { get; set; } = string.Empty;
    public string DeploymentStatus { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = string.Empty;
    public string DeploymentUrl { get; set; } = string.Empty;
    public bool IsCurrentDeployment { get; set; } = true;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string RawJson { get; set; } = "{}";

    public Release Release { get; set; } = null!;
}
