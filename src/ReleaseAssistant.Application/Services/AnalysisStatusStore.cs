using System.Collections.Concurrent;
using ReleaseAssistant.Application.Models.Responses;

namespace ReleaseAssistant.Application.Services;

public interface IAnalysisStatusStore
{
    void SetStatus(Guid releaseId, AnalysisStatusResponse status);
    AnalysisStatusResponse? GetStatus(Guid releaseId);
}

public sealed class InMemoryAnalysisStatusStore : IAnalysisStatusStore
{
    private readonly ConcurrentDictionary<Guid, AnalysisStatusResponse> _statuses = new();

    public void SetStatus(Guid releaseId, AnalysisStatusResponse status) =>
        _statuses[releaseId] = status;

    public AnalysisStatusResponse? GetStatus(Guid releaseId) =>
        _statuses.TryGetValue(releaseId, out var status) ? status : null;
}
