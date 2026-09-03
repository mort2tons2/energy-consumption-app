namespace StromForbrok.Api.Domain
{
    public class ConsumptionReading
    {
        public DateTime TimestampUtc { get; set; }
        public decimal Kwh { get; set; }
    }
}
