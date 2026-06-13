namespace ReleaseAssistant.Domain.Entities;

public class ToolCallLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReleaseId { get; set; }
    public Guid? AgentRunId { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string InputJson { get; set; } = "{}";
    public string OutputJson { get; set; } = "{}";
    public string Status { get; set; } = string.Empty;
    public string? ErrorCode { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public long DurationMs { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Release Release { get; set; } = null!;
    public AgentRun? AgentRun { get; set; }
}
