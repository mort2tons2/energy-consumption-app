using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StromForbrok.Api.Data;

namespace StromForbrok.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class StatusController(StromForbrokDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<object> Get(CancellationToken cancellationToken)
    {
        var databaseConnected = await db.Database.CanConnectAsync(cancellationToken);

        return new
        {
            status = "ok",
            databaseProvider = db.Database.ProviderName,
            databaseConnected,
            timeUtc = DateTimeOffset.UtcNow,
        };
    }
}
