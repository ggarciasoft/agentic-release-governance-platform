namespace ReleaseAssistant.Application.Models.Responses;

public record AnalysisStatusResponse(
    string ReleaseId,
    string Status,
    int Progress,
    DateTime LastUpdatedAt,
    IReadOnlyList<string> Warnings);

public record AnalysisStepResult(
    string Step,
    int AttachedCount,
    IReadOnlyList<string> Warnings);

public record FullAnalysisResult(
    string ReleaseId,
    string Status,
    IReadOnlyList<AnalysisStepResult> Steps,
    IReadOnlyList<string> Warnings);
