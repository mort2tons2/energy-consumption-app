using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

//   dotnet run --project StromForbrok/StromForbrok.AppHost                        -> SQL Server container (default)
//   dotnet run --project StromForbrok/StromForbrok.AppHost -- --database azure    -> Azure SQL (connection string from Key Vault, read by the API)
//   dotnet run --project StromForbrok/StromForbrok.AppHost -- --database sqlite   -> SQLite file (offline / quick boot)
var mode = (builder.Configuration["database"] ?? builder.Configuration["UseAzure"])?.ToLowerInvariant() switch
{
    "azure" or "true" => Database.Azure,
    "sqlite" => Database.Sqlite,
    "sqlserver" or "false" => Database.SqlServerContainer,
    null when !builder.Environment.IsDevelopment() => Database.Azure,
    _ => Database.SqlServerContainer,
};

var api = builder.AddProject<Projects.StromForbrok_Api>("stromforbrok-api")
    .WithEnvironment("Database__Provider", mode == Database.Sqlite ? "Sqlite" : "SqlServer")
    .WithUrlForEndpoint("https", u => { u.Url = "/swagger"; u.DisplayText = "Swagger"; });

switch (mode)
{
    case Database.SqlServerContainer:
        {
            var container = AddSqlServerContainer(builder);
            api.WithReference(container).WaitFor(container);
            break;
        }

    case Database.Sqlite:
        {
            api.WithReference(AddSqliteDatabase(builder));
            break;
        }

    case Database.Azure:
        {
            if (builder.ExecutionContext.IsRunMode)
            {
                var vaultUri = builder.Configuration["KeyVault:VaultUri"]
                    ?? throw new InvalidOperationException(
                        "Set KeyVault:VaultUri (user-secrets or KeyVault__VaultUri) to use --database azure.");
                api.WithEnvironment("KeyVault__VaultUri", vaultUri);
            }
            break;
        }
}

builder.AddViteApp("stromforbrok-web", "../StromForbrok-webapp/stromforbrok-web")
    .WithReference(api)
    .WaitFor(api)
    .WithEndpoint("http", e => e.Port = 5173, createIfNotExists: false)
    .WithExternalHttpEndpoints();

builder.Build().Run();


static IResourceBuilder<IResourceWithConnectionString> AddSqlServerContainer(IDistributedApplicationBuilder builder)
{
    var password = builder.AddParameter("sql-password", secret: true);

    return builder.AddAzureSqlServer("sql")
        .RunAsContainer(container =>
        {
            container.WithHostPort(1433);
            container.WithPassword(password);
            container.WithDataVolume("stromforbrok-sql-data");
        })
        .AddDatabase("stromforbrokdb");
}

static IResourceBuilder<IResourceWithConnectionString> AddSqliteDatabase(IDistributedApplicationBuilder builder)
{
    var dataDir = Path.Combine(builder.AppHostDirectory, "..", ".data");
    Directory.CreateDirectory(dataDir);

    var path = Path.Combine(dataDir, "stromforbrokdb.sqlite");

    return builder.AddConnectionString("stromforbrokdb", ReferenceExpression.Create($"Data Source={path}"));
}


enum Database
{
    SqlServerContainer,
    Azure,
    Sqlite,
}
