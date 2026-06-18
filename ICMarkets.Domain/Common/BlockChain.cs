namespace ICMarkets.Domain.Common;

public sealed record BlockChain
{
    private BlockChain(CoinType coin, string network, string apiPath)
    {
        Coin = coin;
        Network = network;
        ApiPath = apiPath;
    }

    public CoinType Coin { get; }
    public string Network { get; }

    public string ApiPath { get; }
    public string BlockChainIdentifier => $"{Coin.ToString().ToLowerInvariant()}-{Network}";

    public static readonly BlockChain BtcMain = new(CoinType.Btc, "main", "btc/main");
    public static readonly BlockChain BtcTest3 = new(CoinType.Btc, "test3", "btc/test3");
    public static readonly BlockChain EthMain = new(CoinType.Eth, "main", "eth/main");
    public static readonly BlockChain DashMain = new(CoinType.Dash, "main", "dash/main");
    public static readonly BlockChain LtcMain = new(CoinType.Ltc, "main", "ltc/main");

    public static IReadOnlyList<BlockChain> All { get; } =
    [
        EthMain, DashMain, BtcMain, BtcTest3, LtcMain
    ];

    public static BlockChain? FromIdentifier(string? identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return null;
        }

        return All.FirstOrDefault(c =>
            string.Equals(c.BlockChainIdentifier, identifier, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsSupported(string? identifier) => FromIdentifier(identifier) is not null;

    public override string ToString() => BlockChainIdentifier;
}