using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReleaseAssistant.Application.Interfaces;
using ReleaseAssistant.Infrastructure.Data;
using ReleaseAssistant.Infrastructure.Repositories;

namespace ReleaseAssistant.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(connectionString));
        services.AddScoped<IReleaseRepository, ReleaseRepository>();
        services.AddScoped<IApplicationMappingRepository, ApplicationMappingRepository>();
        return services;
    }
}
