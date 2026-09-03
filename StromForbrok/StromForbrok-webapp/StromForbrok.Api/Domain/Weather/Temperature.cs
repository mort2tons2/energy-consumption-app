namespace StromForbrok.Api.Domain.Weather
{
    public class Temperature
    {
        public string StationId { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public decimal Value { get; set; }
    }
}
