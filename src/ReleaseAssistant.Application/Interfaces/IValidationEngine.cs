using ReleaseAssistant.Application.Models.Responses;
using ReleaseAssistant.Domain.Entities;

namespace ReleaseAssistant.Application.Interfaces;

public interface IValidationEngine
{
    Task<ValidationSummary> ValidateAsync(Release release, CancellationToken ct = default);
}
