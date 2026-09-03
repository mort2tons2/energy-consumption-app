namespace StromForbrok.Api.Infrastructure.Weather;

using Microsoft.Extensions.Options;
using StromForbrok.Api.Configuration;
using StromForbrok.Api.Domain.Weather;
using System.Globalization;
using System.Net;
using System.Text.Json;

public class WeatherClient(HttpClient http, IOptions<FrostOptions> options) : IWeatherClient
{
    private readonly FrostOptions _options = options.Value;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<IReadOnlyList<DailyTemperature>> GetDailyTemperaturesAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        var referenceTime = $"{from:yyyy-MM-dd}/{to.AddDays(1):yyyy-MM-dd}";

        var url =
            $"observations/v0.jsonld?sources={Uri.EscapeDataString(_options.StationId)}" +
            $"&elements={Uri.EscapeDataString("mean(air_temperature P1D)")}" +
            $"&referencetime={Uri.EscapeDataString(referenceTime)}";

        using var response = await http.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return [];

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<WeatherResponse>(stream, JsonOptions, cancellationToken);

        return (payload?.Data ?? [])
            .Where(d => d.Observations.Count > 0)
            .Select(d => new DailyTemperature
            {
                Date = DateOnly.FromDateTime(DateTime.Parse(
                    d.ReferenceTime, CultureInfo.InvariantCulture)),
                TemperatureCelcius = d.Observations[0].Value,
            })
            .OrderBy(t => t.Date)
            .ToList();
    }
}
