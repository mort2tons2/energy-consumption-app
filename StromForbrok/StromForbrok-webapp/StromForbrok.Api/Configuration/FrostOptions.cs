namespace StromForbrok.Api.Configuration;

public sealed class FrostOptions
{
    public const string SectionName = "Frost";

    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";

    public string StationId { get; set; } = "";
}
