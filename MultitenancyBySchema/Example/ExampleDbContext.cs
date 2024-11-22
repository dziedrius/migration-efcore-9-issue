using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using MultitenancyBySchema.Example.Entities;
using MultitenancyBySchema.Infrastructure.MultitenancySupport;
using Npgsql;

namespace MultitenancyBySchema.Example;

public class ExampleDbContext(
    DbContextOptions<ExampleDbContext> options,
    ITenantProvider tenantProvider,
    NpgsqlDataSource dataSource)
    : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // can't do this in dependency injection config, as tenant provider does not have tenant set yet
        optionsBuilder
            .UseNpgsql(dataSource,
                o => o.MigrationsHistoryTable(HistoryRepository.DefaultTableName, tenantProvider.DbSchemaName));
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<Enum>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(tenantProvider.DbSchemaName);

        base.OnModelCreating(modelBuilder);
    }
    
    public DbSet<Topic> Topics => Set<Topic>();
}