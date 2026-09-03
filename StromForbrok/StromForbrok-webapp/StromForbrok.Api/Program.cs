using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StromForbrok.Api.Configuration;
using StromForbrok.Api.Data;
using StromForbrok.Api.Infrastructure.EnergyConsumption;
using StromForbrok.Api.Infrastructure.Weather;
using StromForbrok.Api.Services;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
if (!string.IsNullOrWhiteSpace(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
}

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

var useSqlite = string.Equals(builder.Configuration["Database:Provider"], "Sqlite", StringComparison.OrdinalIgnoreCase);

if (useSqlite)
{
    var connectionString = builder.Configuration.GetConnectionString("stromforbrokdb")
        ?? "Data Source=stromforbrokdb.sqlite";

    builder.Services.AddDbContext<StromForbrokDbContext>(options =>
    {
        options.UseSqlite(connectionString);
    });
}
else
{
    var connectionString =
        (builder.Configuration["db-dev-connection-string"]
         ?? builder.Configuration.GetConnectionString("stromforbrokdb")
         ?? throw new InvalidOperationException(
             "No SQL Server connection string. Expected KV secret 'db-dev-connection-string' or ConnectionStrings:stromforbrokdb."))
        .Trim();

    builder.Services.AddDbContext<StromForbrokDbContext>(options =>
    {
        options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure());
    });
}

builder.Services.AddHostedService<DbInitializer>();

builder.Services.Configure<FrostOptions>(o =>
{
    o.ClientId = builder.Configuration["weather-client-id"] ?? o.ClientId;
    o.ClientSecret = builder.Configuration["weather-client-sec"] ?? o.ClientSecret;
    o.StationId = builder.Configuration["weather-station-id"] ?? o.StationId;
});

builder.Services.Configure<ElviaOptions>(o =>
{
    o.BaseUrl = builder.Configuration["EnergyConsumptionBaseUrl"] ?? o.BaseUrl;
    o.AccessToken = builder.Configuration["energy-consumption"] ?? o.AccessToken;
    o.MeteringPointId = builder.Configuration["energy-m-point"] ?? o.MeteringPointId;
});

builder.Services.AddOptions<DashboardOptions>().Bind(builder.Configuration.GetSection(DashboardOptions.SectionName));

builder.Services.AddHttpClient<IWeatherClient, WeatherClient>((serviceProvider, httpClient) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<FrostOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.ClientId))
    {
        throw new InvalidOperationException("Frost:ClientId is not configured.");
    }

    httpClient.BaseAddress = new Uri($"{builder.Configuration["FrostBaseUrl"]}");

    var basic = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{options.ClientId}:"));
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient<IEnergyConsumptionClient, EnergyConsumptionClient>((serviceProvider, httpClient) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<ElviaOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.BaseUrl) || string.IsNullOrWhiteSpace(options.AccessToken))
    {
        throw new InvalidOperationException("Elvia:BaseUrl and Elvia:AccessToken must be configured.");
    }

    httpClient.BaseAddress = new Uri(options.BaseUrl);

    httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true, };
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.AccessToken);
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddSingleton<IDegreeDayCalculator, DegreeDayCalculator>();
builder.Services.AddScoped<SyncService>();
builder.Services.AddScoped<DashboardQueryService>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
