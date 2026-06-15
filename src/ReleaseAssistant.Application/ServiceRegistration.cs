using Microsoft.Extensions.DependencyInjection;
using ReleaseAssistant.Application.Interfaces;
using ReleaseAssistant.Application.Services;
using ReleaseAssistant.Application.Validation;

namespace ReleaseAssistant.Application;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IValidationEngine, ValidationEngine>();
        services.AddScoped<ReleaseService>();
        services.AddScoped<ReleaseAnalysisService>();
        services.AddSingleton<IAnalysisStatusStore, InMemoryAnalysisStatusStore>();
        services.AddScoped<ApplicationMappingService>();
        services.AddScoped<IToolCallLogger, ToolCallLoggerService>();
        return services;
    }
}
