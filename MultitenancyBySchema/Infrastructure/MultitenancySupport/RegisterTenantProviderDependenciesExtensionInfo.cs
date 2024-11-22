using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MultitenancyBySchema.Infrastructure.MultitenancySupport;

public class RegisterTenantProviderDependenciesExtensionInfo : DbContextOptionsExtensionInfo
{
    public RegisterTenantProviderDependenciesExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
    {
    }

    public override int GetServiceProviderHashCode() => 0;

    public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) =>
        string.Equals(LogFragment, other.LogFragment, StringComparison.Ordinal);
    
    public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
    {
    }

    public override bool IsDatabaseProvider => false;
    public override string LogFragment => nameof(RegisterTenantProviderDependenciesExtensionInfo);
}