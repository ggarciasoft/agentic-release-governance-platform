using ReleaseAssistant.Application;
using ReleaseAssistant.AzureDevOps;
using ReleaseAssistant.Infrastructure;
using ReleaseAssistant.McpServer.Tools;
using ReleaseAssistant.Agents;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=release-assistant.db";

builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString)
    .AddAgents()
    .AddAzureDevOps(builder.Configuration);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly(typeof(ReleaseGovernanceTools).Assembly);

var app = builder.Build();

// Ensure DB is created and seeded on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReleaseAssistant.Infrastructure.Data.AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    db.Database.EnsureCreated();
    await ReleaseAssistant.Infrastructure.Data.DbSeeder.SeedAsync(db, config);
}

app.MapMcp();
app.Run();
