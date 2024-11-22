namespace MultitenancyBySchema.Infrastructure.MultitenancySupport;

public class TenantProvider : ITenantProvider
{
    public string DbSchemaName => TenantContext.CurrentTenant!;

    public IDisposable BeginScope(string tenant)
    {
        return TenantContext.BeginScope(tenant);
    }

    public override string ToString()
    {
        return DbSchemaName;
    }
}