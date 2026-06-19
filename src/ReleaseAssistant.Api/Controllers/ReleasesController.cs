using Microsoft.AspNetCore.Mvc;
using ReleaseAssistant.Application.Models.Mcp;
using ReleaseAssistant.Application.Models.Requests;
using ReleaseAssistant.Application.Models.Responses;
using ReleaseAssistant.Application.Services;

namespace ReleaseAssistant.Api.Controllers;

[ApiController]
[Route("api/releases")]
public class ReleasesController(ReleaseService releaseService, ReleaseAnalysisService analysisService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReleaseRequest req, CancellationToken ct)
    {
        var (release, created) = await releaseService.CreateAsync(req, ct);
        if (!created)
            return Ok(new { id = release.Id, name = release.Name, status = release.Status.ToString(), created = false });
        return CreatedAtAction(nameof(GetById), new { releaseId = release.Id },
            new { id = release.Id, name = release.Name, status = release.Status.ToString(), created = true });
    }

    [HttpGet("{releaseId:guid}")]
    public async Task<IActionResult> GetById(Guid releaseId, CancellationToken ct)
    {
        var release = await releaseService.GetAsync(releaseId, ct);
        if (release == null) return NotFound(Problem("Release not found.", statusCode: 404));
        return Ok(ReleaseDetailMapper.ToDetailResponse(release));
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var releases = await releaseService.ListAsync(ct);
        return Ok(releases.Select(r => new { r.Id, r.Name, r.ChangeRequest, r.Status, r.CreatedAt }));
    }

    [HttpDelete("{releaseId:guid}")]
    public async Task<IActionResult> Delete(Guid releaseId, CancellationToken ct)
    {
        try
        {
            await releaseService.SoftDeleteAsync(releaseId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // Attach endpoints (agent-driven, backs MCP attach_* tools)

    [HttpPost("{releaseId}/work-items")]
    public async Task<IActionResult> AttachWorkItems(string releaseId,
        [FromBody] AttachWorkItemsRequest req, CancellationToken ct)
    {
        try
        {
            var count = await releaseService.AttachWorkItemsAsync(req with { ReleaseId = releaseId }, ct);
            return Ok(new { attachedCount = count });
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{releaseId}/pull-requests")]
    public async Task<IActionResult> AttachPullRequests(string releaseId,
        [FromBody] AttachPullRequestsRequest req, CancellationToken ct)
    {
        try
        {
            var count = await releaseService.AttachPullRequestsAsync(req with { ReleaseId = releaseId }, ct);
            return Ok(new { attachedCount = count });
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{releaseId:guid}/applications")]
    public async Task<IActionResult> EnsureApplications(Guid releaseId,
        [FromBody] IReadOnlyList<string> applications, CancellationToken ct)
    {
        try
        {
            var release = await releaseService.GetAsync(releaseId, ct);
            if (release == null) return NotFound();
            var count = await releaseService.EnsureApplicationsAsync(
                releaseId, applications, release.Organization, release.Project, ct);
            return Ok(new { addedCount = count });
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{releaseId:guid}/applications/sync")]
    public async Task<IActionResult> SyncApplicationsFromMappings(Guid releaseId, CancellationToken ct)
    {
        try
        {
            var count = await releaseService.SyncApplicationsFromMappingsAsync(releaseId, ct);
            return Ok(new { syncedCount = count });
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{releaseId}/deployments")]
    public async Task<IActionResult> AttachDeployments(string releaseId,
        [FromBody] AttachDeploymentsRequest req, CancellationToken ct)
    {
        try
        {
            var count = await releaseService.AttachDeploymentsAsync(req with { ReleaseId = releaseId }, ct);
            return Ok(new { attachedCount = count });
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{releaseId}/rollback-candidates")]
    public async Task<IActionResult> AttachRollbackCandidates(string releaseId,
        [FromBody] AttachRollbackCandidatesRequest req, CancellationToken ct)
    {
        try
        {
            var count = await releaseService.AttachRollbackCandidatesAsync(req with { ReleaseId = releaseId }, ct);
            return Ok(new { attachedCount = count });
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{releaseId:guid}/rollback-candidates/discover")]
    public async Task<IActionResult> DiscoverRollbackCandidates(Guid releaseId, CancellationToken ct)
    {
        try
        {
            var candidates = await releaseService.FindRollbackCandidatesAsync(releaseId, ct);
            return Ok(new
            {
                rollbackCandidates = candidates.Select(c => new
                {
                    c.ApplicationName,
                    releaseName = c.AzureDevOpsReleaseName,
                    c.EnvironmentName,
                    status = c.DeploymentStatus,
                    url = c.RollbackUrl
                }),
                note = "Returns rollback candidates already attached to this release. " +
                       "Use POST /analyze/rollback or attach manually to collect from Azure DevOps first."
            });
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // Analyze endpoints (server-driven ADO collection + attach)

    [HttpPost("{releaseId:guid}/analyze")]
    public async Task<IActionResult> AnalyzeAll(Guid releaseId, CancellationToken ct)
    {
        try
        {
            var result = await analysisService.AnalyzeAllAsync(releaseId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{releaseId:guid}/analyze/work-items")]
    public async Task<IActionResult> AnalyzeWorkItems(Guid releaseId, CancellationToken ct)
    {
        try
        {
            var result = await analysisService.AnalyzeWorkItemsAsync(releaseId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{releaseId:guid}/analyze/pull-requests")]
    public async Task<IActionResult> AnalyzePullRequests(Guid releaseId, CancellationToken ct)
    {
        try
        {
            var result = await analysisService.AnalyzePullRequestsAsync(releaseId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{releaseId:guid}/analyze/deployments")]
    public async Task<IActionResult> AnalyzeDeployments(Guid releaseId, CancellationToken ct)
    {
        try
        {
            var result = await analysisService.AnalyzeDeploymentsAsync(releaseId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{releaseId:guid}/analyze/rollback")]
    public async Task<IActionResult> AnalyzeRollback(Guid releaseId, CancellationToken ct)
    {
        try
        {
            var result = await analysisService.AnalyzeRollbackAsync(releaseId, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet("{releaseId:guid}/analysis/status")]
    public IActionResult GetAnalysisStatus(Guid releaseId)
    {
        var status = analysisService.GetAnalysisStatus(releaseId);
        if (status == null)
        {
            return Ok(new AnalysisStatusResponse(
                releaseId.ToString(), "NotStarted", 0, DateTime.UtcNow, Array.Empty<string>()));
        }

        return Ok(status);
    }

    [HttpPost("{releaseId:guid}/validate")]
    public async Task<IActionResult> Validate(Guid releaseId, CancellationToken ct)
    {
        try
        {
            var summary = await releaseService.ValidateAsync(releaseId, ct);
            return Ok(summary);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet("{releaseId:guid}/validation-results")]
    public async Task<IActionResult> GetValidationResults(Guid releaseId, CancellationToken ct)
    {
        var release = await releaseService.GetAsync(releaseId, ct);
        if (release == null) return NotFound();
        return Ok(new
        {
            releaseId = release.Id,
            results = release.ValidationResults.Select(v => new
            {
                v.RuleCode, severity = v.Severity.ToString(), status = v.Status.ToString(), v.Message
            })
        });
    }

    [HttpGet("{releaseId:guid}/package")]
    public async Task<IActionResult> GetPackage(Guid releaseId, CancellationToken ct)
    {
        try
        {
            var package = await releaseService.GeneratePackageAsync(releaseId, ct);
            return Ok(package);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{releaseId:guid}/documents/generate")]
    [Produces("text/markdown")]
    public async Task<IActionResult> GenerateDocument(Guid releaseId,
        [FromBody] GenerateDocumentRequest? req,
        [FromServices] ReleaseAssistant.Application.Interfaces.IDocumentGenerator generator,
        CancellationToken ct)
    {
        try
        {
            var format = req?.Format ?? "markdown";
            if (!format.Equals("markdown", StringComparison.OrdinalIgnoreCase))
                return BadRequest(Problem("Only markdown format is supported.", statusCode: 400));

            var package = await releaseService.GeneratePackageAsync(releaseId, ct);
            var content = generator.Generate(package);
            var fileName = BuildReleaseDocumentFileName(package.Release);

            Response.Headers.ContentDisposition = $"inline; filename=\"{fileName}\"";
            return Content(content, "text/markdown; charset=utf-8");
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    private static string BuildReleaseDocumentFileName(ReleasePackageRelease release)
    {
        var changeRequest = string.IsNullOrWhiteSpace(release.ChangeRequest) ? "release" : release.ChangeRequest;
        var environment = string.IsNullOrWhiteSpace(release.TargetEnvironment) ? "production" : release.TargetEnvironment;
        var safeName = string.Concat($"{changeRequest}-{environment}".Select(c =>
            char.IsLetterOrDigit(c) || c == '-' ? c : '-'));
        return $"release-{safeName}.md";
    }

    [HttpPost("{releaseId:guid}/documents")]
    public async Task<IActionResult> SaveDocument(Guid releaseId,
        [FromBody] SaveDocumentRequest req, CancellationToken ct)
    {
        try
        {
            var doc = await releaseService.SaveDocumentAsync(releaseId, req.Format, req.Content, "api", ct);
            return CreatedAtAction(nameof(GetById), new { releaseId },
                new { documentId = doc.Id, version = doc.Version, status = "Saved" });
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet("{releaseId:guid}/documents")]
    public async Task<IActionResult> GetDocuments(Guid releaseId, CancellationToken ct)
    {
        var release = await releaseService.GetAsync(releaseId, ct);
        if (release == null) return NotFound();
        return Ok(release.Documents.Select(d => new { d.Id, d.Version, d.Format, d.GeneratedAt }));
    }
}

public record GenerateDocumentRequest(string Format = "markdown");

public record SaveDocumentRequest(string Format, string Content);
