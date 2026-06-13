using ReleaseAssistant.Application.Interfaces;
using ReleaseAssistant.Domain.Entities;

namespace ReleaseAssistant.Application.Services;

public class ToolCallLoggerService(IReleaseRepository releases) : IToolCallLogger
{
    public async Task LogAsync(Guid releaseId, string toolName, string inputJson, string outputJson,
        string status, long durationMs, string? errorCode = null, string calledBy = "mcp", CancellationToken ct = default)
    {
        var release = await releases.GetByIdAsync(releaseId, ct);
        if (release == null) return;

        release.ToolCallLogs?.Add(new ToolCallLog
        {
            ReleaseId = releaseId,
            ToolName = toolName,
            InputJson = inputJson,
            OutputJson = outputJson,
            Status = status,
            ErrorCode = errorCode,
            StartedAt = DateTime.UtcNow.AddMilliseconds(-durationMs),
            CompletedAt = DateTime.UtcNow,
            DurationMs = durationMs,
            CreatedBy = calledBy
        });

        await releases.SaveChangesAsync(ct);
    }
}
