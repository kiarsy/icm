using FluentAssertions;
using FluentValidation;
using ICMarkets.Application.Abstractions;
using ICMarkets.Application.Common.Behaviors;
using ICMarkets.Domain.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace ICMarkets.UnitTests.Application;

public class MediaRPipelineBehaviorTests
{
    public sealed record RetriableRequest(string Value) : IRequest<string>, IRequestRetriable;

    public sealed record PlainRequest(string Value) : IRequest<string>;

    private sealed class AlwaysFailValidator : AbstractValidator<RetriableRequest>
    {
        public AlwaysFailValidator() => RuleFor(x => x.Value).Must(_ => false).WithMessage("always invalid");
    }

    [Fact]
    public async Task Throws_validation_exception_and_skips_handler_when_invalid()
    {
        var behavior = new MediaRPipelineBehavior<RetriableRequest, string>(
            [new AlwaysFailValidator()],
            NullLogger<MediaRPipelineBehavior<RetriableRequest, string>>.Instance);

        var handlerCalls = 0;
        var act = async () => await behavior.Handle(
            new RetriableRequest("x"),
            () => { handlerCalls++; return Task.FromResult("ok"); },
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        handlerCalls.Should().Be(0);
    }

    [Fact]
    public async Task Retries_a_retriable_request_once_then_succeeds()
    {
        var behavior = new MediaRPipelineBehavior<RetriableRequest, string>(
            [], NullLogger<MediaRPipelineBehavior<RetriableRequest, string>>.Instance);

        var calls = 0;
        var result = await behavior.Handle(
            new RetriableRequest("x"),
            () =>
            {
                calls++;
                if (calls == 1)
                {
                    throw new ConcurrentException("ENTITY");
                }

                return Task.FromResult("ok");
            },
            CancellationToken.None);

        result.Should().Be("ok");
        calls.Should().Be(2);
    }

    [Fact]
    public async Task Does_not_retry_a_non_retriable_request()
    {
        var behavior = new MediaRPipelineBehavior<PlainRequest, string>(
            [], NullLogger<MediaRPipelineBehavior<PlainRequest, string>>.Instance);

        var calls = 0;
        var act = async () => await behavior.Handle(
            new PlainRequest("x"),
            () => { calls++; throw new ConcurrentException("boom"); },
            CancellationToken.None);

        await act.Should().ThrowAsync<ConcurrentException>();
        calls.Should().Be(1);
    }
}
