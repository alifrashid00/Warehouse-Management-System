namespace StockFlow.Infrastructure.Ai;

public class AiSettings
{
    public const string SectionName = "Ai";

    public string BaseUrl { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;

    /// <summary>Comes from user-secrets or environment variables, never from appsettings.json.</summary>
    public string ApiKey { get; set; } = string.Empty;
}
