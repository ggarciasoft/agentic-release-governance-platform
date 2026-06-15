using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ReleaseAssistant.AzureDevOps.Models;

namespace ReleaseAssistant.AzureDevOps;

public class AzureDevOpsClient(HttpClient http, IOptions<AzureDevOpsOptions> options)
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);
    private readonly AzureDevOpsOptions _opts = options.Value;

    private string OrgUrl => $"{_opts.BaseUrl}/{_opts.Organization}";
    private string ProjectUrl => $"{OrgUrl}/{_opts.Project}";

    public async Task<IReadOnlyList<AdoWorkItem>> SearchWorkItemsByTagAsync(
        string tag, string? org = null, string? project = null, CancellationToken ct = default)
    {
        var orgToUse = org ?? _opts.Organization;
        var projToUse = project ?? _opts.Project;

        var wiql = new
        {
            query = $"""
                SELECT [System.Id]
                FROM WorkItems
                WHERE [System.TeamProject] = '{projToUse}'
                AND [System.Tags] CONTAINS '{tag}'
                ORDER BY [System.ChangedDate] DESC
                """
        };

        var wiqlUrl = $"{_opts.BaseUrl}/{orgToUse}/{projToUse}/_apis/wit/wiql?api-version=7.1";
        var wiqlResponse = await PostJsonAsync<AdoWiqlResult>(wiqlUrl, wiql, ct);
        if (wiqlResponse?.WorkItems == null || wiqlResponse.WorkItems.Count == 0)
            return Array.Empty<AdoWorkItem>();

        var ids = string.Join(",", wiqlResponse.WorkItems.Select(w => w.Id));
        var detailUrl = $"{_opts.BaseUrl}/{orgToUse}/_apis/wit/workitems?ids={ids}&$expand=relations&api-version=7.1";
        var detailResponse = await GetAsync<AdoBatchWorkItemResponse>(detailUrl, ct);
        return detailResponse?.Value ?? Array.Empty<AdoWorkItem>();
    }

    public async Task<IReadOnlyList<AdoWorkItem>> GetWorkItemsByIdsAsync(
        IReadOnlyList<int> ids, string? org = null, string? project = null, CancellationToken ct = default)
    {
        if (ids.Count == 0) return Array.Empty<AdoWorkItem>();

        var orgToUse = org ?? _opts.Organization;
        var idsParam = string.Join(",", ids);
        var detailUrl = $"{_opts.BaseUrl}/{orgToUse}/_apis/wit/workitems?ids={idsParam}&$expand=relations&api-version=7.1";
        var detailResponse = await GetAsync<AdoBatchWorkItemResponse>(detailUrl, ct);
        return detailResponse?.Value ?? Array.Empty<AdoWorkItem>();
    }

    public async Task<AdoEnvironmentDeployment?> FindLatestEnvironmentDeploymentAsync(
        int releaseDefinitionId, string environmentName, string? org = null, string? project = null,
        CancellationToken ct = default)
    {
        var releases = await GetReleasesForDefinitionAsync(releaseDefinitionId, environmentName, org, project, ct);
        foreach (var release in releases.OrderByDescending(r => r.CreatedOn))
        {
            var env = FindEnvironment(release, environmentName);
            if (env != null)
                return ToEnvironmentDeployment(release, env);
        }

        return null;
    }

    public async Task<AdoEnvironmentDeployment?> FindPriorSuccessfulDeploymentAsync(
        int releaseDefinitionId, string environmentName, int? excludeReleaseId, string? org = null,
        string? project = null, CancellationToken ct = default)
    {
        var releases = await GetReleasesForDefinitionAsync(releaseDefinitionId, environmentName, org, project, ct);
        foreach (var release in releases.OrderByDescending(r => r.CreatedOn))
        {
            if (excludeReleaseId.HasValue && release.Id == excludeReleaseId.Value)
                continue;

            var env = FindEnvironment(release, environmentName);
            if (env != null && IsSuccessfulEnvironment(env))
                return ToEnvironmentDeployment(release, env);
        }

        return null;
    }

    private static AdoReleaseEnvironment? FindEnvironment(AdoRelease release, string environmentName) =>
        release.Environments?.FirstOrDefault(e =>
            e.Name.Equals(environmentName, StringComparison.OrdinalIgnoreCase));

    private static bool IsSuccessfulEnvironment(AdoReleaseEnvironment env)
    {
        var status = env.Status ?? env.DeploySteps?.LastOrDefault()?.Status ?? string.Empty;
        return status.Equals("succeeded", StringComparison.OrdinalIgnoreCase);
    }

    private static AdoEnvironmentDeployment ToEnvironmentDeployment(AdoRelease release, AdoReleaseEnvironment env)
    {
        var status = env.Status ?? env.DeploySteps?.LastOrDefault()?.Status ?? string.Empty;
        var completedAt = env.DeploySteps?.LastOrDefault()?.LastModifiedOn;
        var url = release.Links?.Web?.Href ?? string.Empty;
        return new AdoEnvironmentDeployment(release, env, status, url, completedAt);
    }

    public async Task<IReadOnlyList<AdoPullRequest>> GetPullRequestsForWorkItemAsync(
        AdoWorkItem workItem, string? org = null, string? project = null, CancellationToken ct = default)
    {
        var orgToUse = org ?? _opts.Organization;
        var projToUse = project ?? _opts.Project;

        var prIds = workItem.Relations?
            .Where(r => r.Rel.Equals("ArtifactLink", StringComparison.OrdinalIgnoreCase)
                && r.Url.Contains("PullRequest", StringComparison.OrdinalIgnoreCase))
            .Select(r => ExtractPrIdFromUrl(r.Url))
            .Where(id => id > 0)
            .Distinct()
            .ToList() ?? new List<int>();

        if (prIds.Count == 0) return Array.Empty<AdoPullRequest>();

        var results = new List<AdoPullRequest>();
        foreach (var prId in prIds)
        {
            // Note: repository is not known from the artifact link alone;
            // a production implementation would resolve via the artifact URI.
            var url = $"{_opts.BaseUrl}/{orgToUse}/{projToUse}/_apis/git/pullrequests/{prId}?api-version=7.1";
            var pr = await GetAsync<AdoPullRequest>(url, ct);
            if (pr != null) results.Add(pr);
        }
        return results;
    }

    public async Task<IReadOnlyList<AdoRelease>> GetReleasesForDefinitionAsync(
        int releaseDefinitionId, string environment, string? org = null, string? project = null,
        CancellationToken ct = default)
    {
        var orgToUse = org ?? _opts.Organization;
        var projToUse = project ?? _opts.Project;

        var url = $"https://vsrm.dev.azure.com/{orgToUse}/{projToUse}/_apis/release/releases"
            + $"?definitionId={releaseDefinitionId}&environmentStatusFilter=6,8"
            + $"&$top=10&api-version=7.1";

        var result = await GetAsync<AdoReleaseList>(url, ct);
        return result?.Value ?? Array.Empty<AdoRelease>();
    }

    private static int ExtractPrIdFromUrl(string url)
    {
        // Artifact link format: vstfs:///GitHub/PullRequest/xxx or .../Git/PullRequest/repoId%2FprId
        var parts = url.Split('/');
        if (parts.Length > 0 && int.TryParse(parts[^1].Split('%')[0], out var id))
            return id;
        return 0;
    }

    private async Task<T?> GetAsync<T>(string url, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        AddAuthHeader(req);
        using var resp = await http.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return default;
        var json = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<T>(json, JsonOpts);
    }

    private async Task<T?> PostJsonAsync<T>(string url, object body, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, url);
        AddAuthHeader(req);
        req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
        using var resp = await http.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return default;
        var json = await resp.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<T>(json, JsonOpts);
    }

    private void AddAuthHeader(HttpRequestMessage req)
    {
        if (string.IsNullOrWhiteSpace(_opts.Pat)) return;
        var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_opts.Pat}"));
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
    }
}

// Internal helper — batch workitem response
file record AdoBatchWorkItemResponse(
    [property: System.Text.Json.Serialization.JsonPropertyName("value")] IReadOnlyList<AdoWorkItem>? Value);
