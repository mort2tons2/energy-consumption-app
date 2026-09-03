using Microsoft.Extensions.Options;
using StromForbrok.Api.Configuration;
using StromForbrok.Api.Domain;
using System.Text.Json;

namespace StromForbrok.Api.Infrastructure.EnergyConsumption;

public class EnergyConsumptionClient(HttpClient http, IOptions<ElviaOptions> options) : IEnergyConsumptionClient
{
    private readonly ElviaOptions _options = options.Value;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<ConsumptionReading>> GetConsumptionAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var start = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var end = to.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var url =
            $"metervalues?startTime={Uri.EscapeDataString(start.ToString("o"))}" +
            $"&endTime={Uri.EscapeDataString(end.ToString("o"))}";

        using var response = await http.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ElviaMeterValuesResponse>(stream, JsonOptions, cancellationToken);

        var point =
            payload?.MeteringPoints.FirstOrDefault(m => m.MeteringPointId == _options.MeteringPointId)
            ?? payload?.MeteringPoints.FirstOrDefault(m => !m.ProductionMeteringPoint)
            ?? payload?.MeteringPoints.FirstOrDefault();

        if (point?.MeterValue is null)
        {
            return [];
        }

        return point.MeterValue.TimeSeries
            .Select(e => new ConsumptionReading
            {
                TimestampUtc = e.StartTime.UtcDateTime,
                Kwh = (decimal)e.Value,
            })
            .OrderBy(r => r.TimestampUtc)
            .ToList();
    }
}
