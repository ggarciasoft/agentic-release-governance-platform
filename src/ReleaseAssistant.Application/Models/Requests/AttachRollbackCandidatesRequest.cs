namespace ReleaseAssistant.Application.Models.Requests;

public record RollbackCandidateData(
    string ApplicationName,
    string ReleaseName,
    string EnvironmentName,
    string Status,
    string Url,
    int? AzureDevOpsReleaseId = null,
    int? ReleaseDefinitionId = null,
    string ReleaseDefinitionName = "",
    DateTime? CompletedAt = null,
    string RawJson = "{}");

public record AttachRollbackCandidatesRequest(
    string ReleaseId,
    IReadOnlyList<RollbackCandidateData> Candidates);
