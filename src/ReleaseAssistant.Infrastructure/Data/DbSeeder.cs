using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ReleaseAssistant.Domain.Entities;

namespace ReleaseAssistant.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IConfiguration configuration)
    {
        var hasValid = await db.ApplicationMappings
            .AnyAsync(m => !string.IsNullOrWhiteSpace(m.ApplicationName));
        if (hasValid)
            return;

        var invalid = await db.ApplicationMappings
            .Where(m => string.IsNullOrWhiteSpace(m.ApplicationName))
            .ToListAsync();
        if (invalid.Count > 0)
            db.ApplicationMappings.RemoveRange(invalid);

        var org = configuration["AzureDevOps:Organization"] ?? "my-org";
        var project = configuration["AzureDevOps:Project"] ?? "my-project";

        var seedPath = FindSeedFile();
        if (seedPath is null)
            return;

        var json = await File.ReadAllTextAsync(seedPath);
        var items = JsonSerializer.Deserialize<List<SeedMapping>>(json) ?? [];

        var mappings = items.Select(item => new ApplicationMapping
        {
            ApplicationName = item.ApplicationName,
            Organization = item.Organization ?? org,
            Project = item.Project ?? project,
            RepositoryName = item.RepositoryName,
            BuildDefinitionId = item.BuildDefinitionId,
            BuildDefinitionName = item.BuildDefinitionName,
            ReleaseDefinitionId = item.ReleaseDefinitionId,
            ReleaseDefinitionName = item.ReleaseDefinitionName,
            ProductionEnvironmentName = item.ProductionEnvironmentName,
            UatEnvironmentName = item.UatEnvironmentName,
            IsActive = item.IsActive
        }).ToList();

        db.ApplicationMappings.AddRange(mappings);
        await db.SaveChangesAsync();
    }

    private static string? FindSeedFile()
    {
        // Try relative to the API project (runtime working dir is usually the API output dir)
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "seed-data.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "seed-data.json"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "seed-data.json"),
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return path;
        }

        return null;
    }

    private sealed record SeedMapping
    {
        public string ApplicationName { get; init; } = string.Empty;
        public string? Organization { get; init; }
        public string? Project { get; init; }
        public string RepositoryName { get; init; } = string.Empty;
        public int? BuildDefinitionId { get; init; }
        public string BuildDefinitionName { get; init; } = string.Empty;
        public int? ReleaseDefinitionId { get; init; }
        public string ReleaseDefinitionName { get; init; } = string.Empty;
        public string ProductionEnvironmentName { get; init; } = string.Empty;
        public string UatEnvironmentName { get; init; } = string.Empty;
        public bool IsActive { get; init; } = true;
    }
}
