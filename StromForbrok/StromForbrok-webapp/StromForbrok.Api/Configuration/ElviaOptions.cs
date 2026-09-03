namespace StromForbrok.Api.Configuration;

public sealed class ElviaOptions
{
    public const string SectionName = "Elvia";

    public string BaseUrl { get; set; } = "";

    public string AccessToken { get; set; } = "";

    public string MeteringPointId { get; set; } = "";
}
