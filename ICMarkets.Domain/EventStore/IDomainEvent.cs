namespace ICMarkets.Domain;

public interface IDomainEvent
{
    string EventId { get; }
    DateTime OccurredAt { get; }
}