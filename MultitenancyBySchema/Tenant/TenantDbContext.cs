using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MultitenancyBySchema.Tenant;

public class TenantDbContext(DbContextOptions<TenantDbContext> options) : DbContext(options)
{
    public const string SchemaName = "tenants";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder
            .UseNpgsql(o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, SchemaName));
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<Tenant> Tenants { get; set; }
}