using Microsoft.Extensions.DependencyInjection;
using ReleaseAssistant.Application.Interfaces;
using ReleaseAssistant.Agents.Documents;

namespace ReleaseAssistant.Agents;

public static class ServiceRegistration
{
    public static IServiceCollection AddAgents(this IServiceCollection services)
    {
        services.AddScoped<IDocumentGenerator, MarkdownDocumentGenerator>();
        return services;
    }
}
