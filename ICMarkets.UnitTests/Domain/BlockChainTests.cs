using FluentAssertions;
using ICMarkets.Domain.Common;

namespace ICMarkets.UnitTests.Domain;

public class BlockChainTests
{
    [Fact]
    public void All_contains_the_five_required_chains()
    {
        BlockChain.All.Select(c => c.BlockChainIdentifier).Should().BeEquivalentTo(
            new[] { "eth-main", "dash-main", "btc-main", "btc-test3", "ltc-main" });
    }

    [Theory]
    [InlineData("btc-main", "btc/main")]
    [InlineData("BTC-MAIN", "btc/main")]
    [InlineData("btc-test3", "btc/test3")]
    [InlineData("eth-main", "eth/main")]
    public void FromIdentifier_resolves_known_chains_case_insensitively(string identifier, string expectedPath)
    {
        var chain = BlockChain.FromIdentifier(identifier);

        chain.Should().NotBeNull();
        chain!.ApiPath.Should().Be(expectedPath);
    }

    [Theory]
    [InlineData("doge-main")]
    [InlineData("")]
    [InlineData(null)]
    public void FromIdentifier_returns_null_and_IsSupported_false_for_unknown_or_empty(string? identifier)
    {
        BlockChain.FromIdentifier(identifier).Should().BeNull();
        BlockChain.IsSupported(identifier).Should().BeFalse();
    }

    [Fact]
    public void Identifier_is_coin_dash_network_lowercased()
    {
        BlockChain.BtcTest3.BlockChainIdentifier.Should().Be("btc-test3");
        BlockChain.BtcTest3.ToString().Should().Be("btc-test3");
    }
}
