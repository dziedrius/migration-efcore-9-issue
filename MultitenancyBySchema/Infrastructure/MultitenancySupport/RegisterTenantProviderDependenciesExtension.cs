using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace MultitenancyBySchema.Infrastructure.MultitenancySupport;

public class RegisterTenantProviderDependenciesExtension : IDbContextOptionsExtension
{
    private readonly ITenantProvider tenantProvider;

    public RegisterTenantProviderDependenciesExtension(ITenantProvider tenantProvider)
    {
        this.tenantProvider = tenantProvider;
        this.Info = new RegisterTenantProviderDependenciesExtensionInfo(this);
    }
    
    public void ApplyServices(IServiceCollection services)
    {
        services.AddSingleton<ITenantProvider>(_ => tenantProvider);
        services.AddSingleton<IModelCacheKeyFactory, DbSchemaAwareModelCacheKeyFactory>();
        services.AddScoped<IMigrationsSqlGenerator, DbSchemaAwareSqlServerMigrationsSqlGenerator>();
    }

    public void Validate(IDbContextOptions options)
    {
    }

    public DbContextOptionsExtensionInfo Info { get; }
}