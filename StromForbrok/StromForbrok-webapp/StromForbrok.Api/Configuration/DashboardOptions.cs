namespace StromForbrok.Api.Configuration;

public sealed class DashboardOptions
{
    public const string SectionName = "Dashboard";
    public int DefaultHistoryDays { get; set; } = 180;
    public double BaseTemperature { get; set; } = 17.0;
}
