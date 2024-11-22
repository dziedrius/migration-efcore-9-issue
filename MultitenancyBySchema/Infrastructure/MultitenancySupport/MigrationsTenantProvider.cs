namespace MultitenancyBySchema.Infrastructure.MultitenancySupport;

public class MigrationsTenantProvider : ITenantProvider
{
    public string? DbSchemaName { get; } = null;
    public IDisposable BeginScope(string tenant)
    {
        throw new NotImplementedException();
    }
}