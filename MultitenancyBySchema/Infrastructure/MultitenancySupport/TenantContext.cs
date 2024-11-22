namespace MultitenancyBySchema.Infrastructure.MultitenancySupport;

internal static class TenantContext
{
    private sealed class ContextDisposable : IDisposable
    {
        private bool disposed;

        public void Dispose()
        {
            if (!disposed)
            {
                EndScope();
                disposed = true;
            }
        }
    }

    private static readonly AsyncLocal<TenantContextHolder> TenantHolder = new();

    /// <summary>
    /// Gets current tenant context
    /// </summary>
    public static string? CurrentTenant => TenantHolder.Value?.Tenant;

    public static IDisposable BeginScope(string tenant)
    {
        // Use an object indirection to hold the OperationContext in the AsyncLocal,
        // so it can be cleared in all ExecutionContexts when its cleared.
        TenantHolder.Value = new TenantContextHolder { Tenant = tenant };

        return new ContextDisposable();
    }

    public static void EndScope()
    {
        var holder = TenantHolder.Value;
        if (holder != null)
        {
            // Clear current Tenant trapped in the AsyncLocals, as its gone
            holder.Tenant = null;
        }
    }

    private class TenantContextHolder
    {
        public string? Tenant { get; set; }
    }
}

public class TenantConfigurationOptions
{
    public const string ConfigKey = "TenantConfiguration";
    public class Tenant
    {
        public string Name { get; set; } = null!;

        public string? ConnectionString { get; set; }
    }

    public string ConnectionString { get; set; } = null!;

    public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
}