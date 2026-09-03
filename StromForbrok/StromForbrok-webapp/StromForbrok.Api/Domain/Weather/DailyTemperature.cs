namespace StromForbrok.Api.Domain.Weather
{
    public class DailyTemperature
    {
        public DateOnly Date { get; set; }
        public double TemperatureCelcius { get; set; }
    }
}
