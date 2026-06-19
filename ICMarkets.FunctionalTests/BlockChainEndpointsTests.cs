using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ICMarkets.Api.ApiModels;
using ICMarkets.Domain.Common;

namespace ICMarkets.FunctionalTests;

public class BlockChainEndpointsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetSupportedChains_returns_the_five_chains()
    {
        var chains = await _client.GetFromJsonAsync<List<BlockchainsNameApiResponse>>("/api/blockchains");

        chains.Should().NotBeNull();
        chains!.Select(c => c.Identifier).Should().BeEquivalentTo(
            new[] { "eth-main", "dash-main", "btc-main", "btc-test3", "ltc-main" });
    }

    [Fact]
    public async Task Refresh_then_latest_returns_the_stored_status()
    {
        var refresh = await _client.PostAsync("/api/blockchains/refresh/btc-main", content: null);
        refresh.StatusCode.Should().Be(HttpStatusCode.Created);

        var latest = await _client.GetFromJsonAsync<BlockchainApiResponse>("/api/blockchains/latest/btc-main");

        latest.Should().NotBeNull();
        latest!.BlockchainIdentifier.Should().Be("btc-main");
        latest.HighFeePerKb.Should().Be(3343);
        latest.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public async Task Refresh_then_history_returns_the_captured_event()
    {
        await _client.PostAsync("/api/blockchains/refresh/eth-main", content: null);

        var response = await _client.GetAsync("/api/blockchains/history/eth-main");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PagedResult<BlockchainSnapshotCapturedApiResponse>>();
        page.Should().NotBeNull();
        page!.Items.Should().NotBeEmpty();
        page.Items.Should().OnlyContain(i => i.EventId == "eth-main");
        page.Items.Should().OnlyContain(i => i.Model != null && i.Model.BlockchainIdentifier == "eth-main");
    }

    [Fact]
    public async Task Refresh_with_unknown_identifier_returns_400_problem_details()
    {
        var response = await _client.PostAsync("/api/blockchains/refresh/doge-main", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Unknown BlockChain");
    }

    [Fact]
    public async Task Latest_for_unknown_stored_identifier_returns_404()
    {
        var response = await _client.GetAsync("/api/blockchains/latest/ltc-main");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Health_endpoints_report_healthy()
    {
        var live = await _client.GetAsync("/health");
        live.StatusCode.Should().Be(HttpStatusCode.OK);

        var ready = await _client.GetAsync("/health/ready");
        ready.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ready.Content.ReadAsStringAsync()).Should().Contain("Healthy");
    }
}
