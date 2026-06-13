using Microsoft.AspNetCore.Mvc;
using ReleaseAssistant.Application.Models.Requests;
using ReleaseAssistant.Application.Services;

namespace ReleaseAssistant.Api.Controllers;

[ApiController]
[Route("api/releases")]
public class ReleasesController(ReleaseService releaseService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReleaseRequest req, CancellationToken ct)
    {
        var release = await releaseService.CreateAsync(req, ct);
        return CreatedAtAction(nameof(GetById), new { releaseId = release.Id },
            new { id = release.Id, name = release.Name, status = release.Status.ToString() });
    }

    [HttpGet("{releaseId:guid}")]
    public async Task<IActionResult> GetById(Guid releaseId, CancellationToken ct)
    {
        var release = await releaseService.GetAsync(releaseId, ct);
        if (release == null) return NotFound(Problem("Release not found.", statusCode: 404));
        return Ok(release);
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

    // Attach endpoints (Copilot-first, backs MCP attach_* tools)

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

    [HttpPost("{releaseId:guid}/rollback-candidates/discover")]
    public async Task<IActionResult> DiscoverRollbackCandidates(Guid releaseId, CancellationToken ct)
    {
        try
        {
            var candidates = await releaseService.FindRollbackCandidatesAsync(releaseId, ct);
            return Ok(candidates);
        }
        catch (KeyNotFoundException) { return NotFound(); }
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
    public async Task<IActionResult> GenerateDocument(Guid releaseId,
        [FromServices] ReleaseAssistant.Application.Interfaces.IDocumentGenerator generator,
        CancellationToken ct)
    {
        try
        {
            var package = await releaseService.GeneratePackageAsync(releaseId, ct);
            var content = generator.Generate(package);
            return Ok(new { format = "markdown", content });
        }
        catch (KeyNotFoundException) { return NotFound(); }
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

public record SaveDocumentRequest(string Format, string Content);
