using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MultitenancyBySchema.Tenant;

// ReSharper disable once UnusedType.Global - used by migrator
public class TenantDbContextFactory : IDesignTimeDbContextFactory<TenantDbContext>
{
    public TenantDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
        return new TenantDbContext(optionsBuilder.Options);
    }
}