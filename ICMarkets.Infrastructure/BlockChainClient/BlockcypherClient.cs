using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using AutoMapper;
using ICMarkets.Application.Abstractions;
using ICMarkets.Domain;
using ICMarkets.Domain.Common;
using ICMarkets.Domain.Common.Exceptions;
using ICMarkets.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ICMarkets.Infrastructure.BlockChainClient;

public class BlockCypherClient(
    HttpClient httpClient,
    IMapper mapper,
    RateLimiter rateLimiter,
    IOptions<BlockCypherOptions> options,
    ILogger<BlockCypherClient> logger)
    : IBlockChainClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<BlockchainModel> GetChainAsync(string requestChainIdentifier,
        CancellationToken cancellationToken)
    {
        using var lease = await rateLimiter.AcquireAsync(
            permitCount: 1,
            cancellationToken);

        if (!lease.IsAcquired)
        {
            throw new BlockCypherRateLimitException();
        }
        var blockChain = BlockChain.FromIdentifier(requestChainIdentifier);
        var path = blockChain.ApiPath;
        if (!string.IsNullOrWhiteSpace(options.Value.Token))
        {
            path += $"?token={options.Value.Token}";
        }

        using var response = await httpClient.GetAsync(path, cancellationToken);
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new BlockCypherTooManyRequestException();
        }
        response.EnsureSuccessStatusCode();

        var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var responseDto = JsonSerializer.Deserialize<BlockCypherResponse>(rawJson, JsonOptions)
                          ?? throw new InvalidOperationException(
                              $"BlockCypher returned an empty body for {requestChainIdentifier}.");
        responseDto.RawJson = rawJson;
        return mapper.Map<BlockchainModel>(responseDto);
    }
}