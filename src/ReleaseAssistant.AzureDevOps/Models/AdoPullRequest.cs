using System.Text.Json.Serialization;

namespace ReleaseAssistant.AzureDevOps.Models;

public record AdoPullRequest(
    [property: JsonPropertyName("pullRequestId")] int PullRequestId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("sourceRefName")] string SourceRefName,
    [property: JsonPropertyName("targetRefName")] string TargetRefName,
    [property: JsonPropertyName("createdBy")] AdoIdentityRef? CreatedBy,
    [property: JsonPropertyName("closedBy")] AdoIdentityRef? ClosedBy,
    [property: JsonPropertyName("creationDate")] DateTime? CreationDate,
    [property: JsonPropertyName("closedDate")] DateTime? ClosedDate,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("repository")] AdoRepository? Repository);

public record AdoRepository(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name);

public record AdoPullRequestList(
    [property: JsonPropertyName("value")] IReadOnlyList<AdoPullRequest> Value);
