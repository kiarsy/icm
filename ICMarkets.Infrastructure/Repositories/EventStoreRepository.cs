using System.Text.Json;
using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Domain;
using ICMarkets.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ICMarkets.Infrastructure.Repositories;

public class EventStoreRepository(IcMarketsDbContext context) : IEventStoreRepository
{

    public async Task AppendAsync(IDomainEvent @event, CancellationToken cancellationToken)
    {
        var envelope = new EventEnvelope
        {
            Id = Guid.NewGuid(),
            EventId = @event.EventId,
            Type = @event.GetType().Name,
            OccurredAt = @event.OccurredAt,
            Payload = @event
        };

        context.Events.Add(envelope);
    }

    public async Task<IReadOnlyList<IDomainEvent>> GetAllHistoryAsync(string? identifier, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.Events
            .AsNoTracking()
            .Where(it => it.Type == nameof(BlockchainSnapshotCaptured));

        if (!string.IsNullOrEmpty(identifier))
        {
            query = query.Where(it => it.EventId == identifier);
        }

        return await query.OrderByDescending(s => s.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(it => it.Payload)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> CountAsync(string identifier, CancellationToken cancellationToken)
    {
        var query = context.Events.AsNoTracking()
            .Where(it => it.Type == nameof(BlockchainSnapshotCaptured));

        if (!string.IsNullOrEmpty(identifier))
        {
            query = query.Where(s => s.EventId == identifier);
        }

        return await query.LongCountAsync(cancellationToken);
    }
}