using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<ProjectDb> Projects => Set<ProjectDb>();
    public DbSet<FlowNodeDb> FlowNodes => Set<FlowNodeDb>();
    public DbSet<FlowNodePropertyDb> FlowNodeProperties => Set<FlowNodePropertyDb>();
    public DbSet<FlowNodeEdgeDb> FlowEdges => Set<FlowNodeEdgeDb>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<ProjectDb>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasMany(x => x.FlowNodes)
                .WithOne(x => x.Project)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.FlowEdges)
                .WithOne(x => x.Project)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FlowNodeDb>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired();

            entity.Property(x => x.Type)
                .IsRequired();

            entity.HasMany(x => x.Properties)
                .WithOne(x => x.FlowNode)
                .HasForeignKey(x => x.FlowNodeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FlowNodePropertyDb>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired();

            entity.Property(x => x.Type)
                .IsRequired();
        });

        builder.Entity<FlowNodeEdgeDb>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SourceId)
                .IsRequired();

            entity.Property(x => x.TargetId)
                .IsRequired();

            entity.HasIndex(x => x.ProjectId);
            entity.HasIndex(x => x.SourceId);
            entity.HasIndex(x => x.TargetId);
        });
    }
}
