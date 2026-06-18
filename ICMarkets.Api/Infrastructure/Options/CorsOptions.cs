namespace ICMarkets.Api.Infrastructure.Options;

public class CorsOptions
{
    public const string SectionName = "Cors";
    public const string PolicyName = "DefaultCorsPolicy";
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}