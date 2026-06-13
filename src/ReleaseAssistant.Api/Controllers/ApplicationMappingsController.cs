using Microsoft.AspNetCore.Mvc;
using ReleaseAssistant.Application.Services;
using ReleaseAssistant.Domain.Entities;

namespace ReleaseAssistant.Api.Controllers;

[ApiController]
[Route("api/application-mappings")]
public class ApplicationMappingsController(ApplicationMappingService mappingService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Ok(await mappingService.ListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var mapping = await mappingService.GetByIdAsync(id, ct);
        return mapping == null ? NotFound() : Ok(mapping);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ApplicationMapping mapping, CancellationToken ct)
    {
        var created = await mappingService.CreateAsync(mapping, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ApplicationMapping update, CancellationToken ct)
    {
        var existing = await mappingService.GetByIdAsync(id, ct);
        if (existing == null) return NotFound();

        existing.ApplicationName = update.ApplicationName;
        existing.Organization = update.Organization;
        existing.Project = update.Project;
        existing.RepositoryId = update.RepositoryId;
        existing.RepositoryName = update.RepositoryName;
        existing.BuildDefinitionId = update.BuildDefinitionId;
        existing.BuildDefinitionName = update.BuildDefinitionName;
        existing.ReleaseDefinitionId = update.ReleaseDefinitionId;
        existing.ReleaseDefinitionName = update.ReleaseDefinitionName;
        existing.ProductionEnvironmentName = update.ProductionEnvironmentName;
        existing.UatEnvironmentName = update.UatEnvironmentName;

        await mappingService.UpdateAsync(existing, ct);
        return Ok(existing);
    }
}
