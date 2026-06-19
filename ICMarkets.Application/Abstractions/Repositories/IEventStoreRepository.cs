using ICMarkets.Domain;

namespace ICMarkets.Application.Abstractions.Repositories;

public interface IEventStoreRepository
{
    Task AppendAsync(IDomainEvent @event, CancellationToken cancellationToken);

    Task<IReadOnlyList<IDomainEvent>>
        GetAllHistoryAsync(string? identifier, int page, int pageSize, CancellationToken cancellationToken);

    Task<long> CountAsync(string identifier, CancellationToken cancellationToken);
}