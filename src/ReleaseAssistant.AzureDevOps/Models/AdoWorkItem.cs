using System.Text.Json.Serialization;

namespace ReleaseAssistant.AzureDevOps.Models;

public record AdoWorkItemFields(
    [property: JsonPropertyName("System.WorkItemType")] string WorkItemType,
    [property: JsonPropertyName("System.Title")] string Title,
    [property: JsonPropertyName("System.State")] string State,
    [property: JsonPropertyName("System.AssignedTo")] AdoIdentityRef? AssignedTo,
    [property: JsonPropertyName("System.Tags")] string? Tags,
    [property: JsonPropertyName("System.AreaPath")] string? AreaPath,
    [property: JsonPropertyName("System.IterationPath")] string? IterationPath);

public record AdoIdentityRef(
    [property: JsonPropertyName("displayName")] string DisplayName);

public record AdoWorkItemRelation(
    [property: JsonPropertyName("rel")] string Rel,
    [property: JsonPropertyName("url")] string Url);

public record AdoWorkItem(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("fields")] AdoWorkItemFields Fields,
    [property: JsonPropertyName("relations")] IReadOnlyList<AdoWorkItemRelation>? Relations);

public record AdoWiqlResult(
    [property: JsonPropertyName("workItems")] IReadOnlyList<AdoWorkItemRef> WorkItems);

public record AdoWorkItemRef(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("url")] string Url);
