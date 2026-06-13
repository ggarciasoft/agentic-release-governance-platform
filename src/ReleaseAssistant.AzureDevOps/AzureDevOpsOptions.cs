namespace ReleaseAssistant.AzureDevOps;

public class AzureDevOpsOptions
{
    public const string Section = "AzureDevOps";

    public string Organization { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string Pat { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://dev.azure.com";
}
