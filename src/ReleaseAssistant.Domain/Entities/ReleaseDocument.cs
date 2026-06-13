namespace ReleaseAssistant.Domain.Entities;

public class ReleaseDocument : EntityBase
{
    public Guid ReleaseId { get; set; }
    public string Format { get; set; } = "markdown";
    public string Content { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public string GeneratedBy { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public Release Release { get; set; } = null!;
}
