using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MultitenancyBySchema.Infrastructure;
using MultitenancyBySchema.Infrastructure.MultitenancySupport;

namespace MultitenancyBySchema.Example;

// ReSharper disable once UnusedType.Global - used by migrator
public class ExampleDbContextFactory : IDesignTimeDbContextFactory<ExampleDbContext>
{
    public ExampleDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ExampleDbContext>();
        var dataSource = DependencyInjection.GetNpgsqlDataSourceBuilder(ConnectionStringProvider.ConnectionString);
        
        return new ExampleDbContext(optionsBuilder.Options, new MigrationsTenantProvider(), dataSource);
    }
}