using ICMarkets.Application.Commands;
using ICMarkets.Domain.Common;
using ICMarkets.Infrastructure.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ICMarkets.Infrastructure.BackgroundJobs;

public class BlockchainPollingService(
    IServiceScopeFactory scopeFactory,
    IOptions<PollingOptions> options,
    ILogger<BlockchainPollingService> logger)
    : BackgroundService
{
    private readonly PollingOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Blockchain polling is disabled;");
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(1, _options.IntervalSeconds)));
        do
        {
            await PollAllChainsAsync(stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task PollAllChainsAsync(CancellationToken cancellationToken)
    {
        var captures = BlockChain.All.Select(blockChain => CaptureAsync(blockChain, cancellationToken));
        await Task.WhenAll(captures);
    }

    private async Task CaptureAsync(BlockChain blockChain, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<ISender>();
            await mediator.Send(new BlockchainPullCommand(blockChain.BlockChainIdentifier), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Operation for {blockChain} was cancelled.", blockChain.BlockChainIdentifier);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to capture snapshot for {blockChain}.", blockChain.BlockChainIdentifier);
        }
    }
}