using System.Text.Json.Serialization;

namespace ICMarkets.Infrastructure.BlockChainClient;

public class BlockCypherResponse
{
    public string Name { get; set; } = string.Empty;
    public long Height { get; set; }
    public string Hash { get; set; } = string.Empty;
    public DateTimeOffset Time { get; set; }

    [JsonPropertyName("latest_url")] public string? LatestUrl { get; set; }
    [JsonPropertyName("previous_hash")] public string? PreviousHash { get; set; }
    [JsonPropertyName("previous_url")] public string? PreviousUrl { get; set; }

    [JsonPropertyName("peer_count")] public long PeerCount { get; set; }

    [JsonPropertyName("unconfirmed_count")]
    public long UnconfirmedCount { get; set; }

    [JsonPropertyName("last_fork_height")] public long? LastForkHeight { get; set; }
    [JsonPropertyName("last_fork_hash")] public string? LastForkHash { get; set; }

    [JsonPropertyName("high_fee_per_kb")] public long? HighFeePerKb { get; set; }

    [JsonPropertyName("medium_fee_per_kb")]
    public long? MediumFeePerKb { get; set; }

    [JsonPropertyName("low_fee_per_kb")] public long? LowFeePerKb { get; set; }

    [JsonPropertyName("high_gas_price")] public long? HighGasPrice { get; set; }
    [JsonPropertyName("medium_gas_price")] public long? MediumGasPrice { get; set; }
    [JsonPropertyName("low_gas_price")] public long? LowGasPrice { get; set; }

    [JsonPropertyName("high_priority_fee")]
    public long? HighPriorityFee { get; set; }

    [JsonPropertyName("medium_priority_fee")]
    public long? MediumPriorityFee { get; set; }

    [JsonPropertyName("low_priority_fee")] public long? LowPriorityFee { get; set; }
    [JsonPropertyName("base_fee")] public long? BaseFee { get; set; }
    public string RawJson { get; set; }
}