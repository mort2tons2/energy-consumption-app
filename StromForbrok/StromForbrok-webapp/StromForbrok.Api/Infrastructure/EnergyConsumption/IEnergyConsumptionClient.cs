using StromForbrok.Api.Domain;

namespace StromForbrok.Api.Infrastructure.EnergyConsumption
{
    public interface IEnergyConsumptionClient
    {
        Task<IReadOnlyList<ConsumptionReading>> GetConsumptionAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken);
    }
}
