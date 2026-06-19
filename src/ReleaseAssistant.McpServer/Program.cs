using Microsoft.Extensions.Hosting;
using ReleaseAssistant.Application;
using ReleaseAssistant.AzureDevOps;
using ReleaseAssistant.Infrastructure;
using ReleaseAssistant.McpServer.Tools;
using ReleaseAssistant.Agents;

var builder = Host.CreateApplicationBuilder(args);

// MCP stdio transport uses stdout exclusively for JSON-RPC; all logs must go to stderr.
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=release-assistant.db";

builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString)
    .AddAgents()
    .AddAzureDevOps(builder.Configuration);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(ReleaseGovernanceTools).Assembly);

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReleaseAssistant.Infrastructure.Data.AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    db.Database.EnsureCreated();
    await ReleaseAssistant.Infrastructure.Data.DbSeeder.SeedAsync(db, config);
}

await host.RunAsync();
