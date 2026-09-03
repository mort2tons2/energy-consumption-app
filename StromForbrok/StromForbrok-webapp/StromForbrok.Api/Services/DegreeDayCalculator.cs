using StromForbrok.Api.Domain.DegreeDays;
using StromForbrok.Api.Domain.Weather;

namespace StromForbrok.Api.Services
{
    public class DegreeDayCalculator : IDegreeDayCalculator
    {
        public IEnumerable<DegreeDayResult> Calculate(
            IEnumerable<DailyTemperature> temperatures,
            double referenceTemperature = 17.0)
        {
            return temperatures
                .Select(t => new DegreeDayResult
                {
                    Date = t.Date,
                    DegreeDays = Math.Max(0, referenceTemperature - t.TemperatureCelcius)
                })
                .ToArray();
        }
    }
}
