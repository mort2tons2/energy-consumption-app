using Microsoft.AspNetCore.Mvc;
using StromForbrok.Api.Services;

namespace StromForbrok.Api.Controllers;

[ApiController]
[Route("weather")]
public class WeatherController(DashboardQueryService query) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<WeatherPoint>> Get(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] Resolution resolution = Resolution.Month,
        CancellationToken ct = default)
    {
        return await query.GetWeatherAsync(from, to, resolution, ct);
    }
}
