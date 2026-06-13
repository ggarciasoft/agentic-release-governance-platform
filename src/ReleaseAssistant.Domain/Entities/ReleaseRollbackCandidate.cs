namespace ReleaseAssistant.Domain.Entities;

public class ReleaseRollbackCandidate : EntityBase
{
    public Guid ReleaseId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public int? AzureDevOpsReleaseId { get; set; }
    public string AzureDevOpsReleaseName { get; set; } = string.Empty;
    public int? ReleaseDefinitionId { get; set; }
    public string ReleaseDefinitionName { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = string.Empty;
    public string DeploymentStatus { get; set; } = string.Empty;
    public string RollbackUrl { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public string RawJson { get; set; } = "{}";

    public Release Release { get; set; } = null!;
}
