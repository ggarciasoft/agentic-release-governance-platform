using System.Text.Json.Serialization;

namespace ReleaseAssistant.AzureDevOps.Models;

public record AdoRelease(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("releaseDefinition")] AdoReleaseDefinitionRef? ReleaseDefinition,
    [property: JsonPropertyName("environments")] IReadOnlyList<AdoReleaseEnvironment>? Environments,
    [property: JsonPropertyName("createdOn")] DateTime? CreatedOn,
    [property: JsonPropertyName("_links")] AdoLinks? Links);

public record AdoReleaseDefinitionRef(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name);

public record AdoReleaseEnvironment(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("deploySteps")] IReadOnlyList<AdoDeployStep>? DeploySteps);

public record AdoDeployStep(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("lastModifiedOn")] DateTime? LastModifiedOn);

public record AdoLinks(
    [property: JsonPropertyName("web")] AdoHref? Web);

public record AdoHref(
    [property: JsonPropertyName("href")] string Href);

public record AdoReleaseList(
    [property: JsonPropertyName("value")] IReadOnlyList<AdoRelease> Value);
