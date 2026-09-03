using Microsoft.AspNetCore.Mvc;
using StromForbrok.Api.Services;

namespace StromForbrok.Api.Controllers;

[ApiController]
[Route("consumption")]
public class ConsumptionController(DashboardQueryService query) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<ConsumptionPoint>> Get(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] Resolution resolution = Resolution.Month,
        CancellationToken cancellationToken = default)
    {
        return await query.GetConsumptionAsync(from, to, resolution, cancellationToken);
    }
}
