using FluentAssertions;
using ICMarkets.Application.Commands;
using ICMarkets.Application.Queries;

namespace ICMarkets.UnitTests.Application;

public class ValidatorTests
{
    private readonly BlockchainPullCommandValidator _pullValidator = new();
    private readonly GetAllHistoryQueryValidator _historyValidator = new();
    private readonly GetLatestStatusQueryValidator _latestValidator = new();

    [Fact]
    public void PullCommand_passes_for_a_supported_chain()
    {
        _pullValidator.Validate(new BlockchainPullCommand("btc-main")).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("doge-main")]
    public void PullCommand_fails_for_empty_or_unsupported(string identifier)
    {
        var result = _pullValidator.Validate(new BlockchainPullCommand(identifier));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(BlockchainPullCommand.BlockchainIdentifier));
    }

    [Fact]
    public void HistoryQuery_allows_null_identifier_but_rejects_unknown()
    {
        _historyValidator.Validate(new GetAllHistoryQuery(1, 10)).IsValid.Should().BeTrue();
        _historyValidator.Validate(new GetAllHistoryQuery(1, 10, "btc-main")).IsValid.Should().BeTrue();
        _historyValidator.Validate(new GetAllHistoryQuery(1, 10, "nope")).IsValid.Should().BeFalse();
    }

    [Fact]
    public void LatestQuery_allows_null_identifier_but_rejects_unknown()
    {
        _latestValidator.Validate(new GetLatestStatusQuery()).IsValid.Should().BeTrue();
        _latestValidator.Validate(new GetLatestStatusQuery("eth-main")).IsValid.Should().BeTrue();
        _latestValidator.Validate(new GetLatestStatusQuery("nope")).IsValid.Should().BeFalse();
    }
}
