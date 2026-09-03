namespace StromForbrok.Api.Infrastructure.EnergyConsumption
{
    public class ElviaMeterValuesResponse
    {
        public List<ElviaMeteringPoint> MeteringPoints { get; set; } = [];
    }

    public class ElviaMeteringPoint
    {
        public string MeteringPointId { get; set; } = string.Empty;
        public bool ProductionMeteringPoint { get; set; }
        public ElviaMeterValue? MeterValue { get; set; }
    }

    public class ElviaMeterValue
    {
        public List<ElviaTimeSeriesEntry> TimeSeries { get; set; } = [];
    }

    public class ElviaTimeSeriesEntry
    {
        public DateTimeOffset StartTime { get; set; }
        public double Value { get; set; }
        public string Uom { get; set; } = string.Empty;
    }
}
