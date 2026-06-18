using ICMarkets.Domain;

namespace ICMarkets.Application.Abstractions.Repositories;

public interface IBlockchainRepository
{
    Task AddAsync(BlockchainModel model, CancellationToken cancellationToken);
    Task<IReadOnlyList<BlockchainModel>> GetLatest(string? identifier, CancellationToken cancellationToken = default);
}