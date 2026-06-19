using System.Net;
using System.Threading.RateLimiting;
using AutoMapper;
using FluentAssertions;
using ICMarkets.Domain.Common.Exceptions;
using ICMarkets.Infrastructure.BlockChainClient;
using ICMarkets.Infrastructure.Mapping;
using ICMarkets.Infrastructure.Options;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ICMarkets.UnitTests.Infrastructure;

public class BlockCypherClientTests
{
    private const string BtcJson = """
    {
      "name": "BTC.main", "height": 954218, "hash": "0000abc", "time": "2026-06-18T09:11:52Z",
      "peer_count": 323, "unconfirmed_count": 3474,
      "high_fee_per_kb": 3343, "medium_fee_per_kb": 1900, "low_fee_per_kb": 1480,
      "last_fork_height": 949205, "last_fork_hash": "0000fork"
    }
    """;

    private const string EthJson = """
    {
      "name": "ETH.main", "height": 25343475, "hash": "2a92", "time": "2026-06-18T09:12:50Z",
      "peer_count": 0, "unconfirmed_count": 24,
      "high_gas_price": 4481153657, "base_fee": 121050288, "medium_priority_fee": 580977771
    }
    """;

    private static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
        return config.CreateMapper();
    }

    private static RateLimiter PermissiveLimiter() => new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
    {
        TokenLimit = 100, TokensPerPeriod = 100, ReplenishmentPeriod = TimeSpan.FromSeconds(1),
        QueueLimit = 100, AutoReplenishment = true
    });

    private static BlockCypherClient CreateClient(HttpStatusCode status, string body, RateLimiter limiter)
    {
        var handler = new StubHandler(status, body);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.blockcypher.com/v1/") };
        var options = Options.Create(new BlockCypherOptions());
        return new BlockCypherClient(httpClient, CreateMapper(), limiter, options,
            NullLogger<BlockCypherClient>.Instance);
    }

    [Fact]
    public async Task Maps_btc_snake_case_fields_and_preserves_raw_json()
    {
        var model = await CreateClient(HttpStatusCode.OK, BtcJson, PermissiveLimiter())
            .GetChainAsync("btc-main", CancellationToken.None);

        model.Name.Should().Be("BTC.main");
        model.Height.Should().Be(954218);
        model.HighFeePerKb.Should().Be(3343);
        model.LowFeePerKb.Should().Be(1480);
        model.LastForkHeight.Should().Be(949205);
        model.BaseFee.Should().BeNull();          // ETH-only field absent for BTC
        model.RawJson.Should().Contain("high_fee_per_kb");
    }

    [Fact]
    public async Task Maps_eth_gas_fields_and_leaves_fee_per_kb_null()
    {
        var model = await CreateClient(HttpStatusCode.OK, EthJson, PermissiveLimiter())
            .GetChainAsync("eth-main", CancellationToken.None);

        model.HighGasPrice.Should().Be(4481153657);
        model.BaseFee.Should().Be(121050288);
        model.MediumPriorityFee.Should().Be(580977771);
        model.HighFeePerKb.Should().BeNull();
    }

    [Fact]
    public async Task Throws_TooManyRequest_on_http_429()
    {
        var act = async () => await CreateClient(HttpStatusCode.TooManyRequests, "{}", PermissiveLimiter())
            .GetChainAsync("btc-main", CancellationToken.None);

        await act.Should().ThrowAsync<BlockCypherTooManyRequestException>();
    }

    [Fact]
    public async Task Throws_RateLimit_when_the_local_limiter_is_exhausted()
    {
        // Drain the bucket so the client's own acquire returns a non-acquired lease.
        var limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 1, TokensPerPeriod = 1, ReplenishmentPeriod = TimeSpan.FromHours(1),
            QueueLimit = 0, AutoReplenishment = false
        });
        var drain = await limiter.AcquireAsync(1);
        drain.IsAcquired.Should().BeTrue();

        var act = async () => await CreateClient(HttpStatusCode.OK, BtcJson, limiter)
            .GetChainAsync("btc-main", CancellationToken.None);

        await act.Should().ThrowAsync<BlockCypherRateLimitException>();
    }

    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
    }
}
