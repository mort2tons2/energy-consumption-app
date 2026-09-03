namespace StromForbrok.Api.Domain
{
    public class Consumption
    {
        public string MeteringPointId { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public decimal Kwh { get; set; }
    }
}
