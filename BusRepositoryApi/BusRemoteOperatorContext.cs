using Microsoft.EntityFrameworkCore;
using Persistence;

namespace BusRepositoryApi;

public class BusRemoteOperatorContext : DbContext
{
    public BusRemoteOperatorContext(DbContextOptions<BusRemoteOperatorContext> contextOptions) : base(contextOptions)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dispatch>()
            .HasOne(d => d.Consumer)
            .WithOne(c => c.Dispatch)
            .HasForeignKey<Consumer>(c => c.DispatchId);
        modelBuilder.Entity<Dispatch>()
            .HasOne(d => d.Producer)
            .WithOne(p=>p.Dispatch)
            .HasForeignKey<Producer>(p=>p.DispatchId);
        modelBuilder.Entity<Consumer>()
            .HasKey(k => k.DispatchId);
        modelBuilder.Entity<Producer>()
            .HasKey(k => k.DispatchId);
    }

    public DbSet<Dispatch> Dispatch { get; set; } = null!;
    public DbSet<Consumer> Consumer { get; set; } = null!;
    public DbSet<Producer> Producer { get; set; } = null!;
}