using ReleaseAssistant.AzureDevOps.Models;

namespace ReleaseAssistant.AzureDevOps;

public record AdoEnvironmentDeployment(
    AdoRelease Release,
    AdoReleaseEnvironment Environment,
    string EnvironmentStatus,
    string ReleaseUrl,
    DateTime? CompletedAt);
