using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StromForbrok.Api.Configuration;
using StromForbrok.Api.Data;
using StromForbrok.Api.Domain.Weather;

namespace StromForbrok.Api.Services;

public enum Resolution
{
    Day,
    Week,
    Month,
    Year,
}

public sealed class DashboardQueryService(
    StromForbrokDbContext db,
    IDegreeDayCalculator degreeDayCalculator,
    IOptions<ElviaOptions> elviaOptions,
    IOptions<FrostOptions> frostOptions,
    IOptions<DashboardOptions> dashboardOptions)
{
    private readonly string _meteringPointId = elviaOptions.Value.MeteringPointId;
    private readonly string _stationId = frostOptions.Value.StationId;
    private readonly DashboardOptions _dashboard = dashboardOptions.Value;

    public async Task<IReadOnlyList<ConsumptionPoint>> GetConsumptionAsync(
        DateOnly? from, DateOnly? to, Resolution resolution, CancellationToken ct)
    {
        var (fromTs, toTs) = ResolveRange(from, to);

        var daily = await db.Consumptions
            .Where(c => c.MeteringPointId == _meteringPointId && c.Timestamp >= fromTs && c.Timestamp < toTs)
            .GroupBy(c => c.Timestamp.Date)
            .Select(g => new { Date = g.Key, Kwh = g.Sum(x => x.Kwh) })
            .ToListAsync(ct);

        return daily
            .GroupBy(d => RangeStart(DateOnly.FromDateTime(d.Date), resolution))
            .OrderBy(g => g.Key)
            .Select(g => new ConsumptionPoint(g.Key, Math.Round((double)g.Sum(x => x.Kwh), 2)))
            .ToList();
    }

    public async Task<IReadOnlyList<WeatherPoint>> GetWeatherAsync(
        DateOnly? from, DateOnly? to, Resolution resolution, CancellationToken ct)
    {
        var (fromTs, toTs) = ResolveRange(from, to);

        var rows = await db.Temperatures
            .Where(t => t.StationId == _stationId && t.Timestamp >= fromTs && t.Timestamp < toTs)
            .OrderBy(t => t.Timestamp)
            .Select(t => new { t.Timestamp, t.Value })
            .ToListAsync(ct);

        var dailyTemps = rows
            .Select(r => new DailyTemperature
            {
                Date = DateOnly.FromDateTime(r.Timestamp),
                TemperatureCelcius = (double)r.Value,
            })
            .ToList();

        var hddByDate = degreeDayCalculator
            .Calculate(dailyTemps, _dashboard.BaseTemperature)
            .ToDictionary(d => d.Date, d => d.DegreeDays);

        return dailyTemps
            .GroupBy(t => RangeStart(t.Date, resolution))
            .OrderBy(g => g.Key)
            .Select(g => new WeatherPoint(
                g.Key,
                Math.Round(g.Average(x => x.TemperatureCelcius), 1),
                Math.Round(g.Sum(x => hddByDate.GetValueOrDefault(x.Date)), 1)))
            .ToList();
    }

    private (DateTime from, DateTime to) ResolveRange(DateOnly? from, DateOnly? to)
    {
        var resolvedTo = to ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var resolvedFrom = from ?? resolvedTo.AddDays(-_dashboard.DefaultHistoryDays);

        return (
            resolvedFrom.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            resolvedTo.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));
    }

    private static DateOnly RangeStart(DateOnly date, Resolution resolution)
    {
        switch (resolution)
        {
            case Resolution.Week:
                var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
                return date.AddDays(-daysSinceMonday);
            case Resolution.Month:
                return new DateOnly(date.Year, date.Month, 1);
            case Resolution.Year:
                return new DateOnly(date.Year, 1, 1);
            default:
                return date;
        }
    }
}

public record ConsumptionPoint(DateOnly PeriodStart, double Kwh);

public record WeatherPoint(DateOnly PeriodStart, double MeanTemperature, double DegreeDays);
