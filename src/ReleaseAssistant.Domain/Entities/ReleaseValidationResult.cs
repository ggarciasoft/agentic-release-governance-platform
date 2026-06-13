using ReleaseAssistant.Domain.Enums;

namespace ReleaseAssistant.Domain.Entities;

public class ReleaseValidationResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReleaseId { get; set; }
    public string RuleCode { get; set; } = string.Empty;
    public FindingSeverity Severity { get; set; }
    public ReadinessStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Release Release { get; set; } = null!;
}
