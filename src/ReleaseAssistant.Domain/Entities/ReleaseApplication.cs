namespace ReleaseAssistant.Domain.Entities;

public class ReleaseApplication : EntityBase
{
    public Guid ReleaseId { get; set; }
    public string ApplicationName { get; set; } = string.Empty;
    public string RepositoryName { get; set; } = string.Empty;
    public int? BuildDefinitionId { get; set; }
    public int? ReleaseDefinitionId { get; set; }
    public string ProductionEnvironmentName { get; set; } = string.Empty;
    public string UatEnvironmentName { get; set; } = string.Empty;

    public Release Release { get; set; } = null!;
}
