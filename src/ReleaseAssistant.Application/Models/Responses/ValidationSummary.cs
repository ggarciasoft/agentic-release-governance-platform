using ReleaseAssistant.Domain.Enums;

namespace ReleaseAssistant.Application.Models.Responses;

public record ValidationFinding(
    string Code,
    FindingSeverity Severity,
    string Message,
    string EntityType = "",
    string EntityId = "");

public record ValidationSummary(
    ReadinessStatus Status,
    IReadOnlyList<ValidationFinding> Blockers,
    IReadOnlyList<ValidationFinding> Warnings,
    IReadOnlyList<ValidationFinding> Info);
