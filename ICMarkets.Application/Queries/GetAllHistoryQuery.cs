using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Application.Common;
using ICMarkets.Domain;
using ICMarkets.Domain.Common;
using MediatR;

namespace ICMarkets.Application.Queries;

public sealed record GetAllHistoryQuery(int Page, int PageSize, string? identifier = null)
    : IRequest<PagedResult<IDomainEvent>>;

public class GetAllHistoryQueryHandler(
    IEventStoreRepository eventStoreRepository)
    : IRequestHandler<GetAllHistoryQuery, PagedResult<IDomainEvent>>
{
    public async Task<PagedResult<IDomainEvent>> Handle(GetAllHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var (page, pageSize) = Pagination.Normalize(request.Page, request.PageSize);
        var items = await eventStoreRepository.GetAllHistoryAsync(request.identifier, page, pageSize,
            cancellationToken);
        var total = await eventStoreRepository.CountAsync(request.identifier, cancellationToken);
        return new PagedResult<IDomainEvent>(items, page, pageSize, total);
    }
}