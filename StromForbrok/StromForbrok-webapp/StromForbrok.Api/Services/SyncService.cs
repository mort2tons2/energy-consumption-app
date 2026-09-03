using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StromForbrok.Api.Configuration;
using StromForbrok.Api.Data;
using StromForbrok.Api.Domain;
using StromForbrok.Api.Domain.Weather;
using StromForbrok.Api.Infrastructure.EnergyConsumption;
using StromForbrok.Api.Infrastructure.Weather;

namespace StromForbrok.Api.Services;

public sealed class SyncService(
    IEnergyConsumptionClient consumptionClient,
    IWeatherClient weatherClient,
    StromForbrokDbContext db,
    IOptions<ElviaOptions> elviaOptions,
    IOptions<FrostOptions> frostOptions,
    ILogger<SyncService> logger)
{
    private readonly string _meteringPointId = elviaOptions.Value.MeteringPointId;
    private readonly string _stationId = frostOptions.Value.StationId;

    public async Task<SyncResult> SyncAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var consumption = await SyncConsumptionAsync(from, to, ct);
        var weather = await SyncWeatherAsync(from, to, ct);
        return new SyncResult(from, to, consumption, weather);
    }

    public async Task<SyncStatus> GetStatusAsync(CancellationToken ct)
    {
        return new SyncStatus(
            await SourceStatusAsync(db.Consumptions.Where(c => c.MeteringPointId == _meteringPointId), c => c.Timestamp, ct),
            await SourceStatusAsync(db.Temperatures.Where(t => t.StationId == _stationId), t => t.Timestamp, ct));
    }

    private async Task<UpsertCount> SyncConsumptionAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var readings = await consumptionClient.GetConsumptionAsync(from, to, ct);

        var fromTs = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(-1);
        var toTs = to.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(2);

        var existing = await db.Consumptions
            .Where(c => c.MeteringPointId == _meteringPointId && c.Timestamp >= fromTs && c.Timestamp < toTs)
            .ToDictionaryAsync(c => c.Timestamp, ct);

        var added = 0;
        var updated = 0;
        foreach (var r in readings)
        {
            if (existing.TryGetValue(r.TimestampUtc, out var row))
            {
                if (row.Kwh != r.Kwh)
                {
                    row.Kwh = r.Kwh;
                    updated++;
                }
            }
            else
            {
                db.Consumptions.Add(new Consumption
                {
                    MeteringPointId = _meteringPointId,
                    Timestamp = r.TimestampUtc,
                    Kwh = r.Kwh,
                });
                added++;
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation(
            "Sync consumption {From}..{To}: fetched {Fetched}, added {Added}, updated {Updated}",
            from, to, readings.Count, added, updated);

        return new UpsertCount(readings.Count, added, updated);
    }

    private async Task<UpsertCount> SyncWeatherAsync(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var temps = await weatherClient.GetDailyTemperaturesAsync(from, to, ct);

        var fromTs = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toTs = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var existing = await db.Temperatures
            .Where(t => t.StationId == _stationId && t.Timestamp >= fromTs && t.Timestamp < toTs)
            .ToDictionaryAsync(t => t.Timestamp, ct);

        var added = 0;
        var updated = 0;
        foreach (var t in temps)
        {
            var ts = t.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var value = (decimal)t.TemperatureCelcius;

            if (existing.TryGetValue(ts, out var row))
            {
                if (row.Value != value)
                {
                    row.Value = value;
                    updated++;
                }
            }
            else
            {
                db.Temperatures.Add(new Temperature { StationId = _stationId, Timestamp = ts, Value = value });
                added++;
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation(
            "Sync weather {From}..{To}: fetched {Fetched}, added {Added}, updated {Updated}",
            from, to, temps.Count, added, updated);

        return new UpsertCount(temps.Count, added, updated);
    }

    private static async Task<SourceStatus> SourceStatusAsync<T>(
        IQueryable<T> query, System.Linq.Expressions.Expression<Func<T, DateTime>> timestamp, CancellationToken ct)
    {
        var count = await query.CountAsync(ct);
        if (count == 0)
        {
            return new SourceStatus(null, null, 0);
        }

        return new SourceStatus(
            await query.MinAsync(timestamp, ct),
            await query.MaxAsync(timestamp, ct),
            count);
    }
}

public record UpsertCount(int Fetched, int Added, int Updated);

public record SyncResult(DateOnly From, DateOnly To, UpsertCount Consumption, UpsertCount Weather);

public record SourceStatus(DateTime? First, DateTime? Last, int Count);

public record SyncStatus(SourceStatus Consumption, SourceStatus Weather);
