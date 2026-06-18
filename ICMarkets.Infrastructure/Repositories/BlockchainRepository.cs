using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Domain;
using ICMarkets.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ICMarkets.Infrastructure.Repositories;

public class BlockchainRepository(IcMarketsDbContext context) : IBlockchainRepository
{
    public async Task AddAsync(BlockchainModel model, CancellationToken cancellationToken)
    {
        var existing = await context.BlockChain
            .FirstOrDefaultAsync(
                s => s.BlockchainIdentifier == model.BlockchainIdentifier,
                cancellationToken);

        if (existing is null)
        {
            model.Revision = 1;
            context.BlockChain.Add(model);
            return;
        }

        existing.UpdatedAt = model.CreatedAt;
        existing.Height = model.Height;
        existing.Time = model.Time;
        existing.LatestUrl = model.LatestUrl;
        existing.PreviousHash = model.PreviousHash;
        existing.PreviousUrl = model.PreviousUrl;
        existing.PeerCount = model.PeerCount;
        existing.UnconfirmedCount = model.UnconfirmedCount;
        existing.LastForkHeight = model.LastForkHeight;
        existing.LastForkHash = model.LastForkHash;
        existing.HighFeePerKb = model.HighFeePerKb;
        existing.MediumFeePerKb = model.MediumFeePerKb;
        existing.LowFeePerKb = model.LowFeePerKb;
        existing.HighGasPrice = model.HighGasPrice;
        existing.MediumGasPrice = model.MediumGasPrice;
        existing.LowGasPrice = model.LowGasPrice;
        existing.HighPriorityFee = model.HighPriorityFee;
        existing.MediumPriorityFee = model.MediumPriorityFee;
        existing.LowPriorityFee = model.LowPriorityFee;
        existing.BaseFee = model.BaseFee;
        existing.RawJson = model.RawJson;
        existing.Revision += 1;
    }

    public async Task<IReadOnlyList<BlockchainModel>> GetLatest(
        string? identifier,
        CancellationToken cancellationToken = default)
    {
        if (identifier != null)
        {
            return await context.BlockChain
                .Where(it=> it.BlockchainIdentifier == identifier)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        return await context.BlockChain
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}