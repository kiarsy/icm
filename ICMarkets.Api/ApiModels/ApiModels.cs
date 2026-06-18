using System.Text.Json.Serialization;

namespace ICMarkets.Api.ApiModels;

public record BlockchainsNameApiResponse(string Identifier, string Coin, string Network, string ApiPath);

public record BlockchainSnapshotCapturedApiResponse
{
    public BlockchainApiResponse Model { get; set; }
    public string EventId { get; set; }
    public DateTime OccurredAt { get; set; }
}

public record BlockchainApiResponse
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BlockchainIdentifier { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? HighFeePerKb { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? MediumFeePerKb { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? LowFeePerKb { get; set; }

//ETH
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? HighGasPrice { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? MediumGasPrice { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? LowGasPrice { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? HighPriorityFee { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? MediumPriorityFee { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? LowPriorityFee { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? BaseFee { get; set; }

    public string RawJson { get; set; } = string.Empty;
    public int Revision { get; set; }
}