using System.Text.Json.Serialization;

namespace ICMarkets.Domain;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(BlockchainSnapshotCaptured), "blockchain-snapshot-captured")]
public interface IDomainEvent
{
    string EventId { get; }
    DateTime OccurredAt { get; }
}