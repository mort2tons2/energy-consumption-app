using StromForbrok.Api.Domain.Weather;

namespace StromForbrok.Api.Infrastructure.Weather
{
    public interface IWeatherClient
    {
        Task<IReadOnlyList<DailyTemperature>> GetDailyTemperaturesAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken);
    }
}
