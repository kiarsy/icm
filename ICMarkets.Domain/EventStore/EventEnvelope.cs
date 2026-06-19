namespace ICMarkets.Domain;

public class EventEnvelope
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public IDomainEvent Payload { get; set; } 
}