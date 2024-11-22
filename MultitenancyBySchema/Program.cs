using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultitenancyBySchema.Infrastructure;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddTenantDbContext();
        services.AddExampleDbContext();
        services.AddHostedService<DbMigratorHostedService>();
    })
    .Build();
    
await host.RunAsync();