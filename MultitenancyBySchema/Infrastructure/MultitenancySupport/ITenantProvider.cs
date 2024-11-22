namespace MultitenancyBySchema.Infrastructure.MultitenancySupport;

public interface ITenantProvider
{
    string? DbSchemaName { get; }

    IDisposable BeginScope(string tenant);
}