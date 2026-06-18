using ICMarkets.Application.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ICMarkets.Application.Commands;

public class BlockchainPullCommandHandler(
    IBlockChainClient blockChainClient,
    IClock clock,
    ILogger<BlockchainPullCommandHandler> logger)
    : IRequestHandler<BlockchainPullCommand>
{
    public async Task Handle(BlockchainPullCommand request, CancellationToken cancellationToken)
    {
        var snapshot = await blockChainClient.GetChainAsync(request.BlockchainIdentifier, cancellationToken);
        snapshot.Id = Guid.NewGuid();
        snapshot.BlockchainIdentifier = request.BlockchainIdentifier;
        snapshot.CreatedAt = clock.UtcNow; 

        //save to db
        logger.LogInformation(
            "Snapshot pulled for {BlockChainIdentifier} ({CreatedAt:o})",
            request.BlockchainIdentifier, snapshot.CreatedAt);
    }
}