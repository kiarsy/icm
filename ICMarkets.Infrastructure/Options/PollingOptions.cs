namespace ICMarkets.Infrastructure.Options;

public class PollingOptions
{
    public const string SectionName = "Polling";
    public bool Enabled { get; set; } = true;
    public int IntervalSeconds { get; set; } = 60;
}