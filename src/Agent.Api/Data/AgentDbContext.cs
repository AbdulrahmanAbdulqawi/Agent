using Microsoft.EntityFrameworkCore;
using Agent.Api.Data.Entities;

namespace Agent.Api.Data;

public class AgentDbContext : DbContext
{
    public AgentDbContext(DbContextOptions<AgentDbContext> options) : base(options)
    {
    }

    public DbSet<AgentEntity> Agents => Set<AgentEntity>();
    public DbSet<AgentRunEntity> Runs => Set<AgentRunEntity>();
    public DbSet<ConversationMessageEntity> Messages => Set<ConversationMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AgentEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Provider).IsRequired().HasMaxLength(50);
            entity.Property(e => e.WorkspacePath).HasMaxLength(500);
            entity.HasIndex(e => e.Name);
            
            entity.HasMany(e => e.Runs)
                .WithOne(r => r.Agent)
                .HasForeignKey(r => r.AgentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AgentRunEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RunId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Provider).HasMaxLength(50);
            entity.Property(e => e.Goal).HasMaxLength(5000);
            entity.Property(e => e.Summary).HasMaxLength(1000);
            entity.Property(e => e.Error).HasMaxLength(2000);
            entity.Property(e => e.ExtraInfoJson).HasColumnType("TEXT");
            entity.HasIndex(e => e.RunId).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartedAt);

            entity.HasMany(e => e.Messages)
                .WithOne(m => m.Run)
                .HasForeignKey(m => m.RunId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConversationMessageEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Text).HasColumnType("TEXT");
            entity.HasIndex(e => e.Sequence);
        });
    }
}
