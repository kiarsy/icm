using ICMarkets.Domain;

namespace ICMarkets.Application.Abstractions.Repositories;

public interface IBlockchainSnapshotRepository
{
    Task AddAsync(BlockchainModel model, CancellationToken cancellationToken);
}