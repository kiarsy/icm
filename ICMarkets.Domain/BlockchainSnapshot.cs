namespace ICMarkets.Domain;

public class BlockchainSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BlockchainIdentifier { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // common to every Block
    public string Name { get; set; } = string.Empty;
    public long Height { get; set; }
    public string Hash { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public string? LatestUrl { get; set; }
    public string? PreviousHash { get; set; }
    public string? PreviousUrl { get; set; }
    public long PeerCount { get; set; }
    public long UnconfirmedCount { get; set; }
    public long? LastForkHeight { get; set; }
    public string? LastForkHash { get; set; }

    // BTC / LTC / DASH
    public long? HighFeePerKb { get; set; }
    public long? MediumFeePerKb { get; set; }
    public long? LowFeePerKb { get; set; }

    //ETH
    public long? HighGasPrice { get; set; }
    public long? MediumGasPrice { get; set; }
    public long? LowGasPrice { get; set; }
    public long? HighPriorityFee { get; set; }
    public long? MediumPriorityFee { get; set; }
    public long? LowPriorityFee { get; set; }
    public long? BaseFee { get; set; }

    public string RawJson { get; set; } = string.Empty;

}