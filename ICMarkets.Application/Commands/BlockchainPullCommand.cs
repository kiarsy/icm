using MediatR;

namespace ICMarkets.Application.Commands;

public sealed record BlockchainPullCommand(string BlockchainIdentifier) : IRequest;