namespace ICMarkets.Infrastructure.Options;

public class BlockCypherOptions
{
    public const string SectionName = "BlockCypher";
    public string? Token { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    
    public int RateLimitTokenLimit { get; set; } = 3;
    public int RateLimitTokensPerPeriod { get; set; } = 3;
    public int RateLimitReplenishmentSeconds { get; set; } = 1;
    public int RateLimitQueueLimit { get; set; } = 100;
}
