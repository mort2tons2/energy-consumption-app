using Microsoft.EntityFrameworkCore;

namespace StromForbrok.Api.Data;

public sealed class DbInitializer(
    IServiceScopeFactory scopeFactory,
    ILogger<DbInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<StromForbrokDbContext>();

        if (db.Database.IsSqlite())
        {
            await db.Database.EnsureCreatedAsync(cancellationToken);
        }
        else
        {
            await db.Database.MigrateAsync(cancellationToken);
        }

        logger.LogInformation("stromforbrokdb schema is up to date");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
