using ReleaseAssistant.Application.Services;

namespace ReleaseAssistant.Tests;

public class AnalysisStatusStoreTests
{
    [Fact]
    public void GetStatus_returns_null_when_not_set()
    {
        var store = new InMemoryAnalysisStatusStore();
        Assert.Null(store.GetStatus(Guid.NewGuid()));
    }

    [Fact]
    public void SetStatus_round_trips()
    {
        var store = new InMemoryAnalysisStatusStore();
        var releaseId = Guid.NewGuid();
        var status = new ReleaseAssistant.Application.Models.Responses.AnalysisStatusResponse(
            releaseId.ToString(), "Completed", 100, DateTime.UtcNow, []);

        store.SetStatus(releaseId, status);

        var loaded = store.GetStatus(releaseId);
        Assert.NotNull(loaded);
        Assert.Equal("Completed", loaded!.Status);
        Assert.Equal(100, loaded.Progress);
    }
}
