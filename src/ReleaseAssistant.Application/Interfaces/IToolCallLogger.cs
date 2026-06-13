namespace ReleaseAssistant.Application.Interfaces;

public interface IToolCallLogger
{
    Task LogAsync(Guid releaseId, string toolName, string inputJson, string outputJson,
        string status, long durationMs, string? errorCode = null, string calledBy = "mcp", CancellationToken ct = default);
}
