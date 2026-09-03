using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StromForbrok.Api.Data;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<StromForbrokDbContext>
{
    public StromForbrokDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<StromForbrokDbContext>()
            .UseSqlServer("Server=localhost;Database=stromforbrokdb;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new StromForbrokDbContext(options);
    }
}
