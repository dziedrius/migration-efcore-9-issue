using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MultitenancyBySchema.Example;
using MultitenancyBySchema.Infrastructure.MultitenancySupport;
using MultitenancyBySchema.Tenant;

namespace MultitenancyBySchema.Infrastructure;

public class DbMigratorHostedService : IHostedService
{
    private readonly ILogger<DbMigratorHostedService> logger;
    private readonly IServiceProvider serviceProvider;
    private readonly ITenantProvider tenantProvider;
    private readonly IHostApplicationLifetime hostApplicationLifetime;

    public DbMigratorHostedService(
        ILogger<DbMigratorHostedService> logger, 
        IServiceProvider serviceProvider,
        ITenantProvider tenantProvider,
        IHostApplicationLifetime hostApplicationLifetime)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
        this.tenantProvider = tenantProvider;
        this.hostApplicationLifetime = hostApplicationLifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await MigrateTenantSchemaAsync();

            await CreateTenants();
            
            await MigrateExampleTenantsAsync();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error when migrating database");
        }

        hostApplicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task MigrateTenantSchemaAsync()
    {
        using var loggerScope = logger.BeginScope("schema={schema}", TenantDbContext.SchemaName);
        logger.LogInformation("Migrating database for schema {schema}", TenantDbContext.SchemaName);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

            await dbContext.Database.MigrateAsync();

            logger.LogInformation("Database was migrated for schema {schema}", TenantDbContext.SchemaName);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error when migrating database for schema {schema}", TenantDbContext.SchemaName);
        }
    }

    private async Task MigrateExampleTenantsAsync()
    {
        var supportedTenants = await GetTenants();
        foreach (var tenant in supportedTenants)
        {
            using var loggerScope = logger.BeginScope("schema={schema}", tenant.GetSchema());
            logger.LogInformation("Migrating database for tenant {tenant}", tenant.Name);

            try
            {
                using var scope = serviceProvider.CreateScope();
                using var tenantContext = tenantProvider.BeginScope(tenant.GetSchema());
                var dbContext = scope.ServiceProvider.GetRequiredService<ExampleDbContext>();
                
                await dbContext.Database.MigrateAsync();

                logger.LogInformation("Database was migrated for tenant {tenant}", tenant.Name);
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, "Error when migrating database for tenant {tenant}", tenant.Name);
            }
        }
    }

    private async Task<IEnumerable<Tenant.Tenant>> GetTenants()
    {
        using var scope = serviceProvider.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        return await dbContext.Tenants.ToListAsync();
    }

    private async Task CreateTenants()
    {
        using var scope = serviceProvider.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();

        var existingTenants = await dbContext.Tenants.ToListAsync();

        if (!existingTenants.Any())
        {
            dbContext.Tenants.Add(new Tenant.Tenant { Id = Guid.NewGuid(), Name = "a-tenant" });
            dbContext.Tenants.Add(new Tenant.Tenant { Id = Guid.NewGuid(), Name = "b-tenant" });
            await dbContext.SaveChangesAsync();
        }
    }
}