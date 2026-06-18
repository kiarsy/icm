using ICMarkets.Application.Abstractions;
using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Domain;
using ICMarkets.Domain.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ICMarkets.Application.Commands;

public class BlockchainPullCommandHandler(
    IBlockChainClient blockChainClient,
    IClock clock,
    IUnitOfWork unitOfWork,
    IBlockchainSnapshotRepository blockchainSnapshotRepository,
    IEventStoreRepository eventStoreRepository,
    ILogger<BlockchainPullCommandHandler> logger)
    : IRequestHandler<BlockchainPullCommand>
{
    public async Task Handle(BlockchainPullCommand request, CancellationToken cancellationToken)
    {
        var snapshot = await blockChainClient.GetChainAsync(request.BlockchainIdentifier, cancellationToken);
        snapshot.Id = Guid.NewGuid();
        snapshot.BlockchainIdentifier = request.BlockchainIdentifier;
        snapshot.CreatedAt = clock.UtcNow;

        await unitOfWork.BeginAsync(cancellationToken);
        await blockchainSnapshotRepository.AddAsync(snapshot, cancellationToken);
        await eventStoreRepository.AppendAsync(new BlockchainSnapshotCaptured(snapshot), cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Snapshot pulled for {BlockChainIdentifier} ({CreatedAt})",
            request.BlockchainIdentifier, snapshot.CreatedAt);
    }

}