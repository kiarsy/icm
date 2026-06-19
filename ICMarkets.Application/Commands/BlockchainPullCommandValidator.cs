using FluentValidation;
using ICMarkets.Domain.Common;

namespace ICMarkets.Application.Commands;

public class BlockchainPullCommandValidator : AbstractValidator<BlockchainPullCommand>
{
    public BlockchainPullCommandValidator()
    {
        RuleFor(x => x.BlockchainIdentifier)
            .NotEmpty().WithMessage("Blockchain Identifier is required.")
            .Must(BlockChain.IsSupported)
            .WithMessage(_ =>
                $"Unknown BlockChain. Supported chains: {string.Join(", ", BlockChain.All.Select(c => c.BlockChainIdentifier))}.");
    }
}
