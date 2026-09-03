using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StromForbrok.Api.Configuration;
using StromForbrok.Api.Services;

namespace StromForbrok.Api.Controllers;

[ApiController]
[Route("sync")]
public class SyncController(
    SyncService sync,
    IOptions<DashboardOptions> dashboardOptions) : ControllerBase
{
    private readonly int _defaultHistoryDays = dashboardOptions.Value.DefaultHistoryDays;

    [HttpPost]
    public async Task<ActionResult<SyncResult>> Post(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        var resolvedTo = to ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var resolvedFrom = from ?? resolvedTo.AddDays(-_defaultHistoryDays);

        if (resolvedTo < resolvedFrom)
        {
            return BadRequest("'to' must not be before 'from'.");
        }

        return Ok(await sync.SyncAsync(resolvedFrom, resolvedTo, ct));
    }

    [HttpGet("status")]
    public async Task<SyncStatus> Status(CancellationToken ct)
    {
        return await sync.GetStatusAsync(ct);
    }
}
