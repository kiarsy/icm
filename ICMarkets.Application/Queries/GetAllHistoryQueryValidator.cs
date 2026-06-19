using FluentValidation;
using ICMarkets.Domain.Common;

namespace ICMarkets.Application.Queries;

public class GetAllHistoryQueryValidator : AbstractValidator<GetAllHistoryQuery>
{
    public GetAllHistoryQueryValidator()
    {
        RuleFor(x => x.identifier)
            .Must(BlockChain.IsSupported)
            .When(x => !string.IsNullOrWhiteSpace(x.identifier))
            .WithMessage(_ =>
                $"Unknown BlockChain. Supported chains: {string.Join(", ", BlockChain.All.Select(c => c.BlockChainIdentifier))}.");
    }
}
