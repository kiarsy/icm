namespace ICMarkets.Infrastructure.Options;

public class BlockCypherOptions
{
    public const string SectionName = "BlockCypher";
    public string BaseUrl { get; set; } = "https://api.blockcypher.com/v1/";
    public string? Token { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
}