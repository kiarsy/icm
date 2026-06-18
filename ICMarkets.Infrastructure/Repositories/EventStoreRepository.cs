using System.Text.Json;
using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Domain;
using ICMarkets.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ICMarkets.Infrastructure.Repositories;

public class EventStoreRepository(IcMarketsDbContext context) : IEventStoreRepository
{
    private static readonly JsonSerializerOptions PayloadOptions = new(JsonSerializerDefaults.Web);

    public async Task AppendAsync(IDomainEvent @event, CancellationToken cancellationToken)
    {
        var nextVersion = await NextVersionAsync(@event.EventId, cancellationToken);
        nextVersion = 1;
        var envelope = new EventEnvelope
        {
            Id = Guid.NewGuid(),
            EventId = @event.EventId,
            Version = nextVersion,
            Type = @event.GetType().Name,
            OccurredAt = @event.OccurredAt,
            Payload = JsonSerializer.Serialize(@event, @event.GetType(), PayloadOptions)
        };

        context.Events.Add(envelope);
    }
    
    private async Task<long> NextVersionAsync(string eventId, CancellationToken cancellationToken)
    {
        var latest = await context.Events
            .Where(e => e.EventId == eventId)
            .Select(e => (long?)e.Version)
            .MaxAsync(cancellationToken);

        return (latest ?? 0) + 1;
    }

}