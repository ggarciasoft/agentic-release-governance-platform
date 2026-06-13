namespace ReleaseAssistant.Application.Models.Requests;

public record WorkItemData(
    int Id,
    string Type,
    string Title,
    string State,
    string AssignedTo = "",
    IReadOnlyList<string>? Tags = null,
    string AreaPath = "",
    string IterationPath = "",
    string Url = "",
    string RawJson = "{}");

public record AttachWorkItemsRequest(
    string ReleaseId,
    IReadOnlyList<WorkItemData> WorkItems);
