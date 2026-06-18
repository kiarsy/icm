namespace ICMarkets.Domain;

public class EventEnvelope
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventId { get; set; } = string.Empty;
    public long Version { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public string Payload { get; set; } = string.Empty;
}