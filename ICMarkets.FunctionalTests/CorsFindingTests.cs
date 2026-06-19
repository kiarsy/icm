using FluentAssertions;

namespace ICMarkets.FunctionalTests;

public class CorsFindingTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Cross_origin_request_gets_Access_Control_Allow_Origin_header()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/blockchains");
        request.Headers.Add("Origin", "https://example.com");

        var response = await _client.SendAsync(request);

        response.IsSuccessStatusCode.Should().BeTrue();
        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeTrue();
    }
}
