using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Domain;
using MediatR;

namespace ICMarkets.Application.Queries;

public sealed record GetLatestStatusQuery(string? identifier = null) : IRequest<IEnumerable<BlockchainModel>>;

public class GetLatestStatusQueryHandler(
    IBlockchainRepository blockchainRepository)
    : IRequestHandler<GetLatestStatusQuery, IEnumerable<BlockchainModel>>
{
    public async Task<IEnumerable<BlockchainModel>> Handle(GetLatestStatusQuery request,
        CancellationToken cancellationToken)
    {
        var latest = await blockchainRepository.GetLatest(request.identifier, cancellationToken);
        return latest;
    }
}