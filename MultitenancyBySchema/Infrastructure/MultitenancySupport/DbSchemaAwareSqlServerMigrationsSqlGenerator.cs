using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Operations;

namespace MultitenancyBySchema.Infrastructure.MultitenancySupport;

internal class DbSchemaAwareSqlServerMigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
{
    private readonly ITenantProvider tenantProvider;
    
    public DbSchemaAwareSqlServerMigrationsSqlGenerator(
        MigrationsSqlGeneratorDependencies dependencies, 
#pragma warning disable EF1001
        INpgsqlSingletonOptions npgsqlSingletonOptions,
#pragma warning restore EF1001
        ITenantProvider tenantProvider)
        : base(dependencies, npgsqlSingletonOptions)
    {
        this.tenantProvider = tenantProvider;
    }

    protected override void Generate(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        ChangeSchema(operation, model);

        base.Generate(operation, model, builder);
    }

    // Well, ef core does not support multitenancy through schemas well, hence some hacking needed
    // There's a good chance that this will be painful when internal API will change
    // insert/update/delete data handling taken from:
    // https://github.com/npgsql/efcore.pg/issues/1603
    // Wish there was a better way to handle that.
    private void ChangeSchema(MigrationOperation? operation, IModel? model)
    {
        if (operation == null)
        {
            return;
        }

        switch (operation)
        {
            case NpgsqlCreateDatabaseOperation _:
            case NpgsqlDropDatabaseOperation _:
                break;
            case EnsureSchemaOperation ensureSchemaOperation:
                ensureSchemaOperation.Name = tenantProvider.DbSchemaName!;
                break;
            case CreateTableOperation createTableOperation:
                createTableOperation.Schema = tenantProvider.DbSchemaName;
                foreach (var foreignKey in createTableOperation.ForeignKeys)
                {
                    ChangeSchema(foreignKey, model);
                }
                break;
            case DropTableOperation dropTableOperation:
                dropTableOperation.Schema = tenantProvider.DbSchemaName;
                break;
            case CreateIndexOperation createIndexOperation:
                createIndexOperation.Schema = tenantProvider.DbSchemaName;
                break;
            case AddColumnOperation addColumnOperation:
                addColumnOperation.Schema = tenantProvider.DbSchemaName;
                break;
            case AlterColumnOperation alterColumnOperation:
                alterColumnOperation.Schema = tenantProvider.DbSchemaName;
                break;
            case DropColumnOperation dropColumnOperation:
                dropColumnOperation.Schema = tenantProvider.DbSchemaName;
                break;
            case RenameColumnOperation renameColumnOperation:
                renameColumnOperation.Schema = tenantProvider.DbSchemaName;
                break;
            case AddForeignKeyOperation addForeignKeyOperation:
                addForeignKeyOperation.Schema = tenantProvider.DbSchemaName;
                addForeignKeyOperation.PrincipalSchema = tenantProvider.DbSchemaName;
                break;
            case DropForeignKeyOperation dropForeignKeyOperation:
                dropForeignKeyOperation.Schema = tenantProvider.DbSchemaName;
                break;
            case RenameTableOperation renameTableOperation:
                renameTableOperation.Schema = tenantProvider.DbSchemaName;
                renameTableOperation.NewSchema = tenantProvider.DbSchemaName;
                break;
            case InsertDataOperation insertDataOperation:
                insertDataOperation.Schema = tenantProvider.DbSchemaName;
                
                if (model == null)
                {
                    throw new ArgumentException($"For some reason model argument was null, check {nameof(DbSchemaAwareSqlServerMigrationsSqlGenerator)} source code for reason (did you update EF Core recently?).");
                }
                
                var table = GetRelationalTable(model, insertDataOperation.Table);
                insertDataOperation.ColumnTypes ??= table.Columns.Select(s => s.StoreType).ToArray();
                break;
            case UpdateDataOperation updateDataOperation:
                updateDataOperation.Schema = tenantProvider.DbSchemaName;

                if (model == null)
                {
                    throw new ArgumentException($"For some reason model argument was null, check {nameof(DbSchemaAwareSqlServerMigrationsSqlGenerator)} source code for reason (did you update EF Core recently?).");
                }
                
                table = GetRelationalTable(model, updateDataOperation.Table);

                var opColumnNames = updateDataOperation.Columns.ToList();
                var opColumns = table.Columns.Where(s => opColumnNames.Contains(s.Name));

                updateDataOperation.ColumnTypes ??= opColumns.Select(s => s.StoreType).ToArray();

                if (table.PrimaryKey == null)
                {
                    throw new ArgumentException($"For some reason table.PrimaryKey was null, check {nameof(DbSchemaAwareSqlServerMigrationsSqlGenerator)} source code for reason (did you update EF Core recently or misconfigured your entity primary key?).");
                }

                updateDataOperation.KeyColumnTypes ??= table.PrimaryKey.Columns.Select(s => s.StoreType).ToArray();

                break;
            case DeleteDataOperation deleteDataOperation:
                deleteDataOperation.Schema = tenantProvider.DbSchemaName;
                
                if (model == null)
                {
                    throw new ArgumentException($"For some reason model argument was null, check {nameof(DbSchemaAwareSqlServerMigrationsSqlGenerator)} source code for reason (did you update EF Core recently?).");
                }
                
                table = GetRelationalTable(model, deleteDataOperation.Table);
                
                if (table.PrimaryKey == null)
                {
                    throw new ArgumentException($"For some reason table.PrimaryKey was null, check {nameof(DbSchemaAwareSqlServerMigrationsSqlGenerator)} source code for reason (did you update EF Core recently or misconfigured your entity primary key?).");
                }

                deleteDataOperation.KeyColumnTypes ??= table.PrimaryKey.Columns.Select(s => s.StoreType).ToArray();
                
                break;
            case DropPrimaryKeyOperation dropPrimaryKeyOperation:
                dropPrimaryKeyOperation.Schema = tenantProvider.DbSchemaName;
                
                break;
            case DropIndexOperation dropIndexOperation:
                dropIndexOperation.Schema = tenantProvider.DbSchemaName;
                
                break;
            case AddPrimaryKeyOperation addPrimaryKeyOperation:
                addPrimaryKeyOperation.Schema = tenantProvider.DbSchemaName;
                
                break;
            case RenameIndexOperation renameIndexOperation:
                renameIndexOperation.Schema = tenantProvider.DbSchemaName;
                
                break;
            // ReSharper disable once UnusedVariable
            case SqlOperation sqlOperation:
                // there's nothing to do                
                break;
            default:
                throw new NotImplementedException(
                    $"Migration operation of type {operation.GetType().Name} is not supported by DbSchemaAwareSqlServerMigrationsSqlGenerator.");
        }
    }
    
    private static ITable GetRelationalTable(IModel model, string tableName)
    {
        var relationalModel = model.GetRelationalModel();

        var table = relationalModel.Tables.FirstOrDefault(s => s.Name == tableName);

        if (table == null)
        {
            throw new InvalidOperationException($"Table {tableName} was not found in the model.");
        }
        
        return table;
    }
}