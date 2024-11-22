using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MultitenancyBySchema.Infrastructure.MultitenancySupport;

internal class DbSchemaAwareModelCacheKeyFactory(ITenantProvider tenantProvider) : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        return Tuple.Create(context.GetType(), tenantProvider.DbSchemaName, designTime);
    }
}