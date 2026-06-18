using ICMarkets.Domain;

namespace ICMarkets.Application.Abstractions;

public interface IBlockChainClient
{
    Task<BlockchainSnapshot> GetChainAsync(string requestChainIdentifier, CancellationToken cancellationToken);
}