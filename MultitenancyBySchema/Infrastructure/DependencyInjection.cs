using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MultitenancyBySchema.Example;
using MultitenancyBySchema.Infrastructure.MultitenancySupport;
using MultitenancyBySchema.Tenant;
using Npgsql;

namespace MultitenancyBySchema.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTenantDbContext(this IServiceCollection serviceCollection)
    {
        var connectionString = ConnectionStringProvider.ConnectionString;
        serviceCollection.AddNpgsql<TenantDbContext>(connectionString);

        return serviceCollection;
    }

    public static IServiceCollection AddExampleDbContext(this IServiceCollection serviceCollection)
    {
        var tenantProvider = new TenantProvider();
        var connectionString = ConnectionStringProvider.ConnectionString;
        serviceCollection.AddSingleton<ITenantProvider>(_ => tenantProvider);

        serviceCollection.AddSingleton(_ => GetNpgsqlDataSourceBuilder(connectionString));

        serviceCollection.AddDbContext<ExampleDbContext>((provider, builder) =>
        {
            var localTenantProvider = provider.GetRequiredService<ITenantProvider>();
            
            ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(
                new RegisterTenantProviderDependenciesExtension(localTenantProvider));
        });

        return serviceCollection;
    }

    public static NpgsqlDataSource GetNpgsqlDataSourceBuilder(string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);

        return builder.Build();
    }
}