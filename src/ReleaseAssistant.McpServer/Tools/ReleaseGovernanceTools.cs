using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol.Server;
using ReleaseAssistant.Application.Models.Requests;
using ReleaseAssistant.Application.Services;
using ReleaseAssistant.Domain.Entities;

namespace ReleaseAssistant.McpServer.Tools;

[McpServerToolType]
public class ReleaseGovernanceTools(
    ReleaseService releaseService,
    ApplicationMappingService mappingService,
    ReleaseAnalysisService analysisService)
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // ─────────────────────────────────────────────────────────────────────────
    // 1. create_release_item
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "create_release_item")]
    [Description("Creates a release item in the Release Assistant system. Returns a releaseId for all subsequent calls.")]
    public async Task<string> CreateReleaseItemAsync(
        [Description("Human-readable release name")] string releaseName,
        [Description("Change Request identifier, e.g. CR-12345")] string changeRequest,
        [Description("Azure DevOps organization slug")] string organization,
        [Description("Azure DevOps project name")] string project,
        [Description("Target deployment environment, e.g. Production")] string targetEnvironment,
        [Description("List of application names included in this release")] IReadOnlyList<string> applications)
    {
        const string tool = "create_release_item";
        try
        {
            var (release, created) = await releaseService.CreateAsync(new CreateReleaseRequest(
                releaseName, changeRequest, organization, project, targetEnvironment, applications));

            var data = new { releaseId = release.Id.ToString(), status = release.Status.ToString(), created };
            var warnings = created
                ? Array.Empty<string>()
                : new[] { $"Existing release found for {changeRequest}; returning releaseId {release.Id}." };
            return Serialize(McpResponse<object>.Ok(tool, data, warnings));
        }
        catch (Exception ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "CREATE_FAILED", ex.Message));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 2. get_release_item
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "get_release_item")]
    [Description("Returns the current release record for a given releaseId.")]
    public async Task<string> GetReleaseItemAsync(
        [Description("The releaseId returned by create_release_item")] string releaseId)
    {
        const string tool = "get_release_item";
        if (!Guid.TryParse(releaseId, out var id))
            return Serialize(McpResponse<object>.Fail(tool, "VALIDATION_ERROR", "releaseId must be a valid GUID."));

        var release = await releaseService.GetAsync(id);
        if (release == null)
            return Serialize(McpResponse<object>.Fail(tool, "RELEASE_NOT_FOUND", $"Release {releaseId} not found."));

        var data = new
        {
            releaseId = release.Id.ToString(),
            name = release.Name,
            changeRequest = release.ChangeRequest,
            organization = release.Organization,
            project = release.Project,
            targetEnvironment = release.TargetEnvironment,
            status = release.Status.ToString(),
            workItemCount = release.WorkItems.Count,
            pullRequestCount = release.PullRequests.Count,
            deploymentCount = release.Deployments.Count,
            rollbackCandidateCount = release.RollbackCandidates.Count
        };
        return Serialize(McpResponse<object>.Ok(tool, data));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 3. get_application_mapping
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "get_application_mapping")]
    [Description("Returns the repository and pipeline mapping for an application name.")]
    public async Task<string> GetApplicationMappingAsync(
        [Description("Application name to look up")] string applicationName,
        [Description("Azure DevOps organization (optional)")] string? organization = null,
        [Description("Azure DevOps project (optional)")] string? project = null)
    {
        const string tool = "get_application_mapping";
        var mapping = await mappingService.GetByNameAsync(applicationName, organization, project);
        if (mapping == null)
            return Serialize(McpResponse<object>.Fail(tool, "MAPPING_NOT_FOUND",
                $"No active mapping found for application '{applicationName}'."));

        var data = new
        {
            applicationName = mapping.ApplicationName,
            repositoryName = mapping.RepositoryName,
            buildDefinitionId = mapping.BuildDefinitionId,
            releaseDefinitionId = mapping.ReleaseDefinitionId,
            productionEnvironmentName = mapping.ProductionEnvironmentName,
            uatEnvironmentName = mapping.UatEnvironmentName
        };
        return Serialize(McpResponse<object>.Ok(tool, data));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 4. attach_work_items_to_release
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "attach_work_items_to_release")]
    [Description("Stores work items collected by the azure-devops MCP server. Pass all work items for the Change Request tag.")]
    public async Task<string> AttachWorkItemsToReleaseAsync(
        [Description("The releaseId")] string releaseId,
        [Description("JSON array of work items with id, type, title, state, url")] string workItemsJson)
    {
        const string tool = "attach_work_items_to_release";
        try
        {
            var items = JsonSerializer.Deserialize<IReadOnlyList<WorkItemData>>(workItemsJson, JsonOpts)
                ?? Array.Empty<WorkItemData>();

            var count = await releaseService.AttachWorkItemsAsync(
                new AttachWorkItemsRequest(releaseId, items));

            return Serialize(McpResponse<object>.Ok(tool, new { attachedCount = count }));
        }
        catch (KeyNotFoundException ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "RELEASE_NOT_FOUND", ex.Message));
        }
        catch (Exception ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "VALIDATION_ERROR", ex.Message));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 5. attach_pull_requests_to_release
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "attach_pull_requests_to_release")]
    [Description("Stores pull requests collected by the azure-devops MCP server.")]
    public async Task<string> AttachPullRequestsToReleaseAsync(
        [Description("The releaseId")] string releaseId,
        [Description("JSON array of pull requests with pullRequestId, repositoryName, title, status, targetBranch, url")] string pullRequestsJson)
    {
        const string tool = "attach_pull_requests_to_release";
        try
        {
            var prs = JsonSerializer.Deserialize<IReadOnlyList<PullRequestData>>(pullRequestsJson, JsonOpts)
                ?? Array.Empty<PullRequestData>();

            var count = await releaseService.AttachPullRequestsAsync(
                new AttachPullRequestsRequest(releaseId, prs));

            return Serialize(McpResponse<object>.Ok(tool, new { attachedCount = count }));
        }
        catch (KeyNotFoundException ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "RELEASE_NOT_FOUND", ex.Message));
        }
        catch (Exception ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "VALIDATION_ERROR", ex.Message));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 6. attach_deployments_to_release
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "attach_deployments_to_release")]
    [Description("Stores deployment candidates when data was collected outside this server. Prefer collect_release_deployments for classic release pipeline discovery.")]
    public async Task<string> AttachDeploymentsToReleaseAsync(
        [Description("The releaseId")] string releaseId,
        [Description("JSON array of deployments with applicationName, releaseName, environmentName, status, url")] string deploymentsJson)
    {
        const string tool = "attach_deployments_to_release";
        try
        {
            var deps = JsonSerializer.Deserialize<IReadOnlyList<DeploymentData>>(deploymentsJson, JsonOpts)
                ?? Array.Empty<DeploymentData>();

            var count = await releaseService.AttachDeploymentsAsync(
                new AttachDeploymentsRequest(releaseId, deps));

            return Serialize(McpResponse<object>.Ok(tool, new { attachedCount = count }));
        }
        catch (KeyNotFoundException ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "RELEASE_NOT_FOUND", ex.Message));
        }
        catch (Exception ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "VALIDATION_ERROR", ex.Message));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 6b. collect_release_deployments
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "collect_release_deployments")]
    [Description("Discovers current deployment candidates from Azure DevOps classic release pipelines for each mapped application and attaches them to the release. Requires AzureDevOps:Pat on the MCP server.")]
    public async Task<string> CollectReleaseDeploymentsAsync(
        [Description("The releaseId")] string releaseId)
    {
        const string tool = "collect_release_deployments";
        if (!Guid.TryParse(releaseId, out var id))
            return Serialize(McpResponse<object>.Fail(tool, "VALIDATION_ERROR", "Invalid releaseId."));

        try
        {
            var result = await analysisService.AnalyzeDeploymentsAsync(id);
            var release = await releaseService.GetAsync(id);
            var deployments = (release?.Deployments ?? [])
                .Select(d => new
                {
                    applicationName = d.ApplicationName,
                    releaseName = d.AzureDevOpsReleaseName,
                    environmentName = d.EnvironmentName,
                    status = d.DeploymentStatus,
                    url = d.DeploymentUrl,
                    isCurrentDeployment = d.IsCurrentDeployment
                })
                .ToArray();

            var data = new
            {
                attachedCount = result.AttachedCount,
                step = result.Step,
                deployments
            };
            return Serialize(McpResponse<object>.Ok(tool, data, result.Warnings));
        }
        catch (KeyNotFoundException ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "RELEASE_NOT_FOUND", ex.Message));
        }
        catch (Exception ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "COLLECTION_FAILED", ex.Message));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 7. find_rollback_candidates
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "find_rollback_candidates")]
    [Description("Returns rollback candidates already attached to the release. Does not query Azure DevOps. Use collect_release_rollback_candidates to discover and attach rollback data first.")]
    public async Task<string> FindRollbackCandidatesAsync(
        [Description("The releaseId")] string releaseId)
    {
        const string tool = "find_rollback_candidates";
        if (!Guid.TryParse(releaseId, out var id))
            return Serialize(McpResponse<object>.Fail(tool, "VALIDATION_ERROR", "Invalid releaseId."));

        try
        {
            var candidates = await releaseService.FindRollbackCandidatesAsync(id);
            var warnings = new List<string>();

            var release = await releaseService.GetAsync(id);
            if (release != null)
            {
                foreach (var app in release.Applications)
                {
                    if (!candidates.Any(c => c.ApplicationName.Equals(app.ApplicationName, StringComparison.OrdinalIgnoreCase)))
                        warnings.Add($"{app.ApplicationName} has no rollback candidate.");
                }
            }

            var data = new
            {
                rollbackCandidates = candidates.Select(c => new
                {
                    applicationName = c.ApplicationName,
                    releaseName = c.AzureDevOpsReleaseName,
                    environmentName = c.EnvironmentName,
                    status = c.DeploymentStatus,
                    url = c.RollbackUrl
                }).ToArray()
            };
            return Serialize(McpResponse<object>.Ok(tool, data, warnings));
        }
        catch (KeyNotFoundException ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "RELEASE_NOT_FOUND", ex.Message));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 7b. collect_release_rollback_candidates
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "collect_release_rollback_candidates")]
    [Description("Discovers prior successful classic release pipeline deployments for rollback and attaches one candidate per mapped application. Requires AzureDevOps:Pat on the MCP server.")]
    public async Task<string> CollectReleaseRollbackCandidatesAsync(
        [Description("The releaseId")] string releaseId)
    {
        const string tool = "collect_release_rollback_candidates";
        if (!Guid.TryParse(releaseId, out var id))
            return Serialize(McpResponse<object>.Fail(tool, "VALIDATION_ERROR", "Invalid releaseId."));

        try
        {
            var result = await analysisService.AnalyzeRollbackAsync(id);
            var release = await releaseService.GetAsync(id);
            var rollbackCandidates = (release?.RollbackCandidates ?? [])
                .Select(c => new
                {
                    applicationName = c.ApplicationName,
                    releaseName = c.AzureDevOpsReleaseName,
                    environmentName = c.EnvironmentName,
                    status = c.DeploymentStatus,
                    url = c.RollbackUrl
                })
                .ToArray();

            var data = new
            {
                attachedCount = result.AttachedCount,
                step = result.Step,
                rollbackCandidates
            };
            return Serialize(McpResponse<object>.Ok(tool, data, result.Warnings));
        }
        catch (KeyNotFoundException ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "RELEASE_NOT_FOUND", ex.Message));
        }
        catch (Exception ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "COLLECTION_FAILED", ex.Message));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 7c. attach_rollback_candidates_to_release
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "attach_rollback_candidates_to_release")]
    [Description("Stores rollback candidates when data was collected outside this server. Prefer collect_release_rollback_candidates for classic release pipeline discovery.")]
    public async Task<string> AttachRollbackCandidatesToReleaseAsync(
        [Description("The releaseId")] string releaseId,
        [Description("JSON array of rollback candidates with applicationName, releaseName, environmentName, status, url")] string candidatesJson)
    {
        const string tool = "attach_rollback_candidates_to_release";
        try
        {
            var candidates = JsonSerializer.Deserialize<IReadOnlyList<RollbackCandidateData>>(candidatesJson, JsonOpts)
                ?? Array.Empty<RollbackCandidateData>();

            var count = await releaseService.AttachRollbackCandidatesAsync(
                new AttachRollbackCandidatesRequest(releaseId, candidates));

            return Serialize(McpResponse<object>.Ok(tool, new { attachedCount = count }));
        }
        catch (KeyNotFoundException ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "RELEASE_NOT_FOUND", ex.Message));
        }
        catch (Exception ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "VALIDATION_ERROR", ex.Message));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 8. validate_release
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "validate_release")]
    [Description("Runs deterministic validation rules against all collected release data. Returns Ready, Warning, Blocked, Incomplete, or Unknown.")]
    public async Task<string> ValidateReleaseAsync(
        [Description("The releaseId")] string releaseId)
    {
        const string tool = "validate_release";
        if (!Guid.TryParse(releaseId, out var id))
            return Serialize(McpResponse<object>.Fail(tool, "VALIDATION_ERROR", "Invalid releaseId."));

        try
        {
            var summary = await releaseService.ValidateAsync(id);
            var data = new
            {
                status = summary.Status.ToString(),
                blockers = summary.Blockers.Select(b => new { code = b.Code, message = b.Message }).ToArray(),
                warnings = summary.Warnings.Select(w => new { code = w.Code, message = w.Message }).ToArray(),
                info = summary.Info.Select(i => new { code = i.Code, message = i.Message }).ToArray()
            };
            return Serialize(McpResponse<object>.Ok(tool, data));
        }
        catch (KeyNotFoundException ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "RELEASE_NOT_FOUND", ex.Message));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 9. generate_release_package
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "generate_release_package")]
    [Description("Builds and returns the full structured release package used by the Release Document Agent.")]
    public async Task<string> GenerateReleasePackageAsync(
        [Description("The releaseId")] string releaseId)
    {
        const string tool = "generate_release_package";
        if (!Guid.TryParse(releaseId, out var id))
            return Serialize(McpResponse<object>.Fail(tool, "VALIDATION_ERROR", "Invalid releaseId."));

        try
        {
            var package = await releaseService.GeneratePackageAsync(id);
            return Serialize(McpResponse<object>.Ok(tool, new { releasePackage = package }));
        }
        catch (KeyNotFoundException ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "RELEASE_NOT_FOUND", ex.Message));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // 10. save_release_document
    // ─────────────────────────────────────────────────────────────────────────

    [McpServerTool(Name = "save_release_document")]
    [Description("Persists a generated release document with automatic versioning.")]
    public async Task<string> SaveReleaseDocumentAsync(
        [Description("The releaseId")] string releaseId,
        [Description("Document format, e.g. markdown")] string format,
        [Description("Full document content")] string content)
    {
        const string tool = "save_release_document";
        if (!Guid.TryParse(releaseId, out var id))
            return Serialize(McpResponse<object>.Fail(tool, "VALIDATION_ERROR", "Invalid releaseId."));

        try
        {
            var doc = await releaseService.SaveDocumentAsync(id, format, content);
            var data = new
            {
                documentId = doc.Id.ToString(),
                version = doc.Version,
                status = "Saved"
            };
            return Serialize(McpResponse<object>.Ok(tool, data));
        }
        catch (KeyNotFoundException ex)
        {
            return Serialize(McpResponse<object>.Fail(tool, "RELEASE_NOT_FOUND", ex.Message));
        }
    }

    private static string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, JsonOpts);
}
