namespace ReleaseAssistant.Application.Models.Requests;

public record CreateReleaseRequest(
    string ReleaseName,
    string ChangeRequest,
    string Organization,
    string Project,
    string TargetEnvironment,
    IReadOnlyList<string> Applications);
