using ICMarkets.Domain;

namespace ICMarkets.Application.Abstractions;

public interface IBlockChainClient
{
    Task<BlockchainModel> GetChainAsync(string requestChainIdentifier, CancellationToken cancellationToken);
}