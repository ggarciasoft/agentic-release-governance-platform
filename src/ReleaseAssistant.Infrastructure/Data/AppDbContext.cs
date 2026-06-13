using Microsoft.EntityFrameworkCore;
using ReleaseAssistant.Domain.Entities;

namespace ReleaseAssistant.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Release> Releases => Set<Release>();
    public DbSet<ReleaseApplication> ReleaseApplications => Set<ReleaseApplication>();
    public DbSet<ApplicationMapping> ApplicationMappings => Set<ApplicationMapping>();
    public DbSet<ReleaseWorkItem> ReleaseWorkItems => Set<ReleaseWorkItem>();
    public DbSet<ReleasePullRequest> ReleasePullRequests => Set<ReleasePullRequest>();
    public DbSet<ReleasePullRequestWorkItem> ReleasePullRequestWorkItems => Set<ReleasePullRequestWorkItem>();
    public DbSet<ReleaseDeployment> ReleaseDeployments => Set<ReleaseDeployment>();
    public DbSet<ReleaseRollbackCandidate> ReleaseRollbackCandidates => Set<ReleaseRollbackCandidate>();
    public DbSet<ReleaseValidationResult> ReleaseValidationResults => Set<ReleaseValidationResult>();
    public DbSet<ReleaseDocument> ReleaseDocuments => Set<ReleaseDocument>();
    public DbSet<AgentRun> AgentRuns => Set<AgentRun>();
    public DbSet<ToolCallLog> ToolCallLogs => Set<ToolCallLog>();
    public DbSet<ValidationRuleConfiguration> ValidationRuleConfigurations => Set<ValidationRuleConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Release>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.ChangeRequest);
            e.HasIndex(r => new { r.Organization, r.Project });
            e.HasQueryFilter(r => r.DeletedAt == null);
            e.HasMany(r => r.Applications).WithOne(a => a.Release).HasForeignKey(a => a.ReleaseId);
            e.HasMany(r => r.WorkItems).WithOne(w => w.Release).HasForeignKey(w => w.ReleaseId);
            e.HasMany(r => r.PullRequests).WithOne(p => p.Release).HasForeignKey(p => p.ReleaseId);
            e.HasMany(r => r.Deployments).WithOne(d => d.Release).HasForeignKey(d => d.ReleaseId);
            e.HasMany(r => r.RollbackCandidates).WithOne(rc => rc.Release).HasForeignKey(rc => rc.ReleaseId);
            e.HasMany(r => r.ValidationResults).WithOne(v => v.Release).HasForeignKey(v => v.ReleaseId);
            e.HasMany(r => r.Documents).WithOne(d => d.Release).HasForeignKey(d => d.ReleaseId);
            e.HasMany(r => r.AgentRuns).WithOne(a => a.Release).HasForeignKey(a => a.ReleaseId);
            e.HasMany(r => r.ToolCallLogs).WithOne(t => t.Release).HasForeignKey(t => t.ReleaseId);
        });

        modelBuilder.Entity<ReleasePullRequestWorkItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.PullRequest).WithMany(p => p.WorkItemLinks).HasForeignKey(x => x.ReleasePullRequestId);
            e.HasOne(x => x.WorkItem).WithMany(w => w.PullRequestLinks).HasForeignKey(x => x.ReleaseWorkItemId);
        });

        modelBuilder.Entity<ApplicationMapping>(e =>
        {
            e.HasIndex(a => a.ApplicationName);
            e.HasIndex(a => a.RepositoryName);
        });

        modelBuilder.Entity<ReleaseWorkItem>(e =>
            e.HasIndex(w => w.AzureDevOpsWorkItemId));

        modelBuilder.Entity<ReleasePullRequest>(e =>
            e.HasIndex(p => p.AzureDevOpsPullRequestId));

        modelBuilder.Entity<ReleaseDeployment>(e =>
            e.HasIndex(d => d.ApplicationName));

        modelBuilder.Entity<ReleaseValidationResult>(e =>
            e.HasIndex(v => v.ReleaseId));

        modelBuilder.Entity<AgentRun>(e =>
            e.HasMany(a => a.ToolCallLogs).WithOne(t => t.AgentRun).HasForeignKey(t => t.AgentRunId).IsRequired(false));
    }
}
