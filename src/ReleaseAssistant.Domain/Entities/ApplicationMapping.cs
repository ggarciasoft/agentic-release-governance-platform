namespace ReleaseAssistant.Domain.Entities;

public class ApplicationMapping : EntityBase
{
    public string ApplicationName { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string? RepositoryId { get; set; }
    public string RepositoryName { get; set; } = string.Empty;
    public int? BuildDefinitionId { get; set; }
    public string BuildDefinitionName { get; set; } = string.Empty;
    public int? ReleaseDefinitionId { get; set; }
    public string ReleaseDefinitionName { get; set; } = string.Empty;
    public string ProductionEnvironmentName { get; set; } = string.Empty;
    public string UatEnvironmentName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
