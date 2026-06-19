using ICMarkets.Application.Abstractions;
using ICMarkets.Domain;
using ICMarkets.Domain.Common;

namespace ICMarkets.FunctionalTests;

public sealed class StubBlockChainClient : IBlockChainClient
{
    private int _counter;

    public Task<BlockchainModel> GetChainAsync(string requestChainIdentifier, CancellationToken cancellationToken)
    {
        var chain = BlockChain.FromIdentifier(requestChainIdentifier);
        var isEth = chain?.Coin == CoinType.Eth;
        var seq = Interlocked.Increment(ref _counter);

        var model = new BlockchainModel
        {
            Name = $"{requestChainIdentifier}",
            Height = 1_000 + seq,
            Hash = $"{requestChainIdentifier}-hash-{seq}",
            Time = new DateTime(2026, 6, 18, 9, 0, 0, DateTimeKind.Utc),
            PeerCount = 7,
            UnconfirmedCount = 3,
            HighFeePerKb = isEth ? null : 3343,
            HighGasPrice = isEth ? 4481153657 : null,
            BaseFee = isEth ? 121050288 : null,
            RawJson = "{\"name\":\"stub\"}"
        };
        return Task.FromResult(model);
    }
}
