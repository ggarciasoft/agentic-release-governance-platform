using ReleaseAssistant.Application;
using ReleaseAssistant.AzureDevOps;
using ReleaseAssistant.Infrastructure;
using ReleaseAssistant.Agents;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Data Source=release-assistant.db";

builder.Services
    .AddApplication()
    .AddInfrastructure(connectionString)
    .AddAgents()
    .AddAzureDevOps(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Release Assistant API", Version = "v1" });
});

var app = builder.Build();

// Ensure DB is created on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReleaseAssistant.Infrastructure.Data.AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
