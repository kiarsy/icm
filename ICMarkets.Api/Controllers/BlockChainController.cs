using AutoMapper;
using ICMarkets.Api.ApiModels;
using ICMarkets.Application.Commands;
using ICMarkets.Application.Common;
using ICMarkets.Application.Queries;
using ICMarkets.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ICMarkets.Api.Controllers;

[ApiController]
[Route("api/blockchains")]
[Produces("application/json")]
public sealed class BlockChainController(ISender mediator, IMapper mapper) : ControllerBase
{
    /// <summary>The catalog of supported blockchains (coin + network).</summary>
    /// <remarks>
    /// Returns every chain the service can capture
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BlockchainsNameApiResponse>), StatusCodes.Status200OK)]
    public IActionResult GetSupportedChains()
    {
        return Ok(BlockChain.All.Select(c => new BlockchainsNameApiResponse(
            c.BlockChainIdentifier,
            c.Coin.ToString(),
            c.Network,
            c.ApiPath
        )));
    }

    /// <summary>The latest captured status for each supported blockchain.</summary>
    /// <remarks>Returns the most recent capture result per blockchain.</remarks>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(IEnumerable<BlockchainApiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<BlockchainApiResponse>>> GetLatestStatus(
        CancellationToken cancellationToken)
    {
        var query = new GetLatestStatusQuery();
        var results = await mediator.Send(query, cancellationToken);
        return Ok(mapper.Map<IEnumerable<BlockchainApiResponse>>(results));
    }

    /// <summary>The latest captured status of an identifier.</summary>
    /// <remarks>Returns the most recent capture result for a blockchain.</remarks>
    [HttpGet("latest/{identifier}")]
    [ProducesResponseType(typeof(BlockchainApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BlockchainApiResponse>> GetLatestStatusOfBlockchain(
        string identifier,
        CancellationToken cancellationToken)
    {
        var query = new GetLatestStatusQuery(identifier);
        var results = await mediator.Send(query, cancellationToken);

        var latest = results.FirstOrDefault();
        if (latest is null)
        {
            return NotFound();
        }

        return Ok(mapper.Map<BlockchainApiResponse>(latest));
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(PagedResult<BlockchainSnapshotCapturedApiResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<BlockchainSnapshotCapturedApiResponse>>> GetAllHistory(
        [FromQuery] int page = Pagination.DefaultPage,
        [FromQuery] int pageSize = Pagination.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAllHistoryQuery(page, pageSize), cancellationToken);
        return Ok(mapper.Map<PagedResult<BlockchainSnapshotCapturedApiResponse>>(result));
    }

    [HttpGet("history/{identifier}")]
    [ProducesResponseType(typeof(PagedResult<BlockchainSnapshotCapturedApiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<BlockchainSnapshotCapturedApiResponse>>> GetEvents(
        string identifier,
        [FromQuery] int page = Pagination.DefaultPage,
        [FromQuery] int pageSize = Pagination.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        //validator
        var result = await mediator.Send(new GetAllHistoryQuery(page, pageSize, identifier), cancellationToken);
        return Ok(mapper.Map<PagedResult<BlockchainSnapshotCapturedApiResponse>>(result));
    }

    /// <summary>Fetches the current reading for a chain from BlockCypher and stores it.</summary>
    [HttpPost("refresh/{identifier}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult> Refresh(
        string identifier, CancellationToken cancellationToken)
    {
        await mediator.Send(new BlockchainPullCommand(identifier), cancellationToken);
        return CreatedAtAction(nameof(GetEvents), new { identifier }, null);
    }
}