using System.Text.Json;
using AutoMapper;
using ICMarkets.Application.Abstractions;
using ICMarkets.Domain;
using ICMarkets.Domain.Common;
using ICMarkets.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace ICMarkets.Infrastructure.BlockChainClient;

public class BlockCypherClient(
    HttpClient httpClient,
    IMapper mapper,
    IOptions<BlockCypherOptions> options)
    : IBlockChainClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<BlockchainSnapshot> GetChainAsync(string requestChainIdentifier,
        CancellationToken cancellationToken)
    {
        var blockChain = BlockChain.FromIdentifier(requestChainIdentifier);
        var path = blockChain.ApiPath;
        if (!string.IsNullOrWhiteSpace(options.Value.Token))
        {
            path += $"?token={options.Value.Token}";
        }

        using var response = await httpClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();

        var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var responseDto = JsonSerializer.Deserialize<BlockCypherResponse>(rawJson, JsonOptions)
                          ?? throw new InvalidOperationException(
                              $"BlockCypher returned an empty body for {requestChainIdentifier}.");

        return mapper.Map<BlockchainSnapshot>(responseDto);
    }
}