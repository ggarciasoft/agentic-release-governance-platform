namespace ReleaseAssistant.McpServer.Tools;

public record McpError(string Code, string Message);

public record McpResponse<T>(
    bool Success,
    string Tool,
    T? Data,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<McpError> Errors)
{
    public static McpResponse<T> Ok(string tool, T data, IReadOnlyList<string>? warnings = null)
        => new(true, tool, data, warnings ?? Array.Empty<string>(), Array.Empty<McpError>());

    public static McpResponse<T> Fail(string tool, string code, string message)
        => new(false, tool, default, Array.Empty<string>(),
            new[] { new McpError(code, message) });
}
