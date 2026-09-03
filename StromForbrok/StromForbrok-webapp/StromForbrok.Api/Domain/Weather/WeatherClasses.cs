namespace StromForbrok.Api.Domain.Weather
{
    public class WeatherResponse
    {
        public List<WeatherDataItem> Data { get; set; } = [];
    }

    public class WeatherDataItem
    {
        public string SourceId { get; set; } = string.Empty;
        public string ReferenceTime { get; set; } = string.Empty;
        public List<WeatherObservation> Observations { get; set; } = [];
    }

    public class WeatherObservation
    {
        public string ElementId { get; set; } = string.Empty;
        public double Value { get; set; }
        public string TimeOffset { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
    }
}
