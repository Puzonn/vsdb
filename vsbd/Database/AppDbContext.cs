using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<ProjectDb> Projects => Set<ProjectDb>();
    public DbSet<ProjectNodeDb> Nodes => Set<ProjectNodeDb>();
    public DbSet<ProjectPinDb> Pins => Set<ProjectPinDb>();
    public DbSet<ProjectEdgeDb> Edges => Set<ProjectEdgeDb>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<ProjectDb>()
            .HasMany(p => p.Nodes)
            .WithOne(n => n.Project)
            .HasForeignKey(n => n.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ProjectNodeDb>()
            .HasMany(n => n.Pins)
            .WithOne(p => p.Node)
            .HasForeignKey(p => p.NodeId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ProjectDb>()
            .HasMany(p => p.Edges)
            .WithOne(e => e.Project)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ProjectEdgeDb>()
            .HasOne(e => e.SourceNode)
            .WithMany()
            .HasForeignKey(e => e.SourceNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<ProjectEdgeDb>()
            .HasOne(e => e.TargetNode)
            .WithMany()
            .HasForeignKey(e => e.TargetNodeId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<ProjectEdgeDb>()
            .HasOne(e => e.SourcePin)
            .WithMany()
            .HasForeignKey(e => e.SourcePinId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<ProjectEdgeDb>()
            .HasOne(e => e.TargetPin)
            .WithMany()
            .HasForeignKey(e => e.TargetPinId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
