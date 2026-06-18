using ICMarkets.Domain;

namespace ICMarkets.Application.Abstractions.Repositories;

public interface IEventStoreRepository
{
    Task AppendAsync(IDomainEvent @event, CancellationToken cancellationToken);
}