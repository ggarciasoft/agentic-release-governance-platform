using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ReleaseAssistant.AzureDevOps;

public static class ServiceRegistration
{
    public static IServiceCollection AddAzureDevOps(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AzureDevOpsOptions>(
            opts => configuration.GetSection(AzureDevOpsOptions.Section).Bind(opts));
        services.AddHttpClient<AzureDevOpsClient>();
        return services;
    }
}
