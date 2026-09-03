using StromForbrok.Api.Domain.DegreeDays;
using StromForbrok.Api.Domain.Weather;

namespace StromForbrok.Api.Services
{
    public interface IDegreeDayCalculator
    {
        IEnumerable<DegreeDayResult> Calculate(IEnumerable<DailyTemperature> temperatures, double referenceTemperature = 17.0);
    }
}
