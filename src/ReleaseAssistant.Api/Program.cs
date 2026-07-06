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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        if (allowedOrigins is { Length: > 0 })
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        else
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Release Assistant API", Version = "v1" });
});

var app = builder.Build();

// Ensure DB is created and seeded on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReleaseAssistant.Infrastructure.Data.AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    db.Database.EnsureCreated();
    await ReleaseAssistant.Infrastructure.Data.DbSeeder.SeedAsync(db, config);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();
app.Run();
