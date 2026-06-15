using ReleaseAssistant.Application.Models.Responses;
using ReleaseAssistant.Domain.Entities;
using ReleaseAssistant.Domain.Enums;

namespace ReleaseAssistant.Tests;

public class ReleaseDetailMapperTests
{
    [Fact]
    public void ToDetailResponse_maps_release_without_navigation_cycles()
    {
        var releaseId = Guid.NewGuid();
        var appId = Guid.NewGuid();
        var release = new Release
        {
            Id = releaseId,
            Name = "CR-3 Production Release",
            ChangeRequest = "CR-3",
            Organization = "ggarciasoftOutlook",
            Project = "release-document",
            TargetEnvironment = "Production",
            Status = ReleaseStatus.Created,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Applications =
            [
                new ReleaseApplication
                {
                    Id = appId,
                    ReleaseId = releaseId,
                    ApplicationName = "release-document",
                    RepositoryName = "release-document",
                    ReleaseDefinitionId = 1,
                    ProductionEnvironmentName = "Production"
                }
            ],
            WorkItems =
            [
                new ReleaseWorkItem
                {
                    Id = Guid.NewGuid(),
                    ReleaseId = releaseId,
                    AzureDevOpsWorkItemId = 3,
                    WorkItemType = "Task",
                    Title = "Sample task",
                    State = "Done",
                    Url = "https://dev.azure.com/example/3"
                }
            ]
        };

        var detail = ReleaseDetailMapper.ToDetailResponse(release);

        Assert.Equal(releaseId, detail.Id);
        Assert.Single(detail.Applications);
        Assert.Equal("release-document", detail.Applications[0].ApplicationName);
        Assert.Single(detail.WorkItems);
        Assert.Equal(3, detail.WorkItems[0].AzureDevOpsWorkItemId);
    }
}
