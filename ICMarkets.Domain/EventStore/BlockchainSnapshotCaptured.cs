namespace ICMarkets.Domain;

public class BlockchainSnapshotCaptured(BlockchainModel model) : IDomainEvent
{
    public BlockchainModel Model { get; } = model;

    public string EventId => Model.BlockchainIdentifier;

    public DateTime OccurredAt => Model.CreatedAt;
}