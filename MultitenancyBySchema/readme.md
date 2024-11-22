# Issue
In dotnet 8 this code run without problem, after migration to dotnet 9, migrations started to fail.
It seems that additional logic was added to check if models are in sync.
And because we adjust schema during running migrations, those models do not match, hence migrations fail.
It is possible to disable this by ignoring warning, but that warning might be useful, when there are actual migrations missing.

# Startup
I've added docker-compose file to run postgres database for testing.
1. Start docker-compose.
2. Start app.

# Migrations
To work with migrations, ef core tools need to be installed"
```shell
dotnet tool install --global dotnet-ef        
```

To create migration for TenantDbContext (replace migration name with our migration name):
```shell
dotnet ef migrations add <MigrationName> --output-dir Tenant/Migrations --context TenantDbContext 
```

To create migration for ExampleDbContext (replace migration name with our migration name):
```shell
dotnet ef migrations add <MigrationName> --output-dir Example/Migrations --context ExampleDbContext
```
