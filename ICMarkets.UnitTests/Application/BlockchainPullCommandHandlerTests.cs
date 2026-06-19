using FluentAssertions;
using ICMarkets.Application.Abstractions;
using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Application.Commands;
using ICMarkets.Domain;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ICMarkets.UnitTests.Application;

public class BlockchainPullCommandHandlerTests
{
    private readonly Mock<IBlockChainClient> _client = new();
    private readonly Mock<IClock> _clock = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IBlockchainRepository> _blockchainRepository = new();
    private readonly Mock<IEventStoreRepository> _eventStoreRepository = new();

    private BlockchainPullCommandHandler CreateHandler() => new(
        _client.Object, _clock.Object, _unitOfWork.Object,
        _blockchainRepository.Object, _eventStoreRepository.Object,
        NullLogger<BlockchainPullCommandHandler>.Instance);

    [Fact]
    public async Task Handle_stamps_snapshot_then_adds_appends_and_commits_once()
    {
        var capturedAt = new DateTime(2026, 6, 18, 9, 0, 0, DateTimeKind.Utc);
        _clock.SetupGet(c => c.UtcNow).Returns(capturedAt);
        _client
            .Setup(c => c.GetChainAsync("btc-main", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlockchainModel { Name = "BTC.main", Height = 100, HighFeePerKb = 42 });

        BlockchainSnapshotCaptured? appended = null;
        _eventStoreRepository
            .Setup(e => e.AppendAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback<IDomainEvent, CancellationToken>((e, _) => appended = e as BlockchainSnapshotCaptured)
            .Returns(Task.CompletedTask);

        await CreateHandler().Handle(new BlockchainPullCommand("btc-main"), CancellationToken.None);

        // Snapshot was stamped with identifier + capture time.
        _blockchainRepository.Verify(r => r.AddAsync(
            It.Is<BlockchainModel>(m => m.BlockchainIdentifier == "btc-main" && m.CreatedAt == capturedAt),
            It.IsAny<CancellationToken>()), Times.Once);

        // Event wraps the same stamped snapshot.
        appended.Should().NotBeNull();
        appended!.Model.BlockchainIdentifier.Should().Be("btc-main");
        appended.EventId.Should().Be("btc-main");

        // Unit of work brackets the writes exactly once.
        _unitOfWork.Verify(u => u.BeginAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_adds_to_read_model_before_committing()
    {
        _clock.SetupGet(c => c.UtcNow).Returns(DateTime.UtcNow);
        _client
            .Setup(c => c.GetChainAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlockchainModel());

        var sequence = new MockSequence();
        _unitOfWork.InSequence(sequence).Setup(u => u.BeginAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _blockchainRepository.InSequence(sequence).Setup(r => r.AddAsync(It.IsAny<BlockchainModel>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _eventStoreRepository.InSequence(sequence).Setup(e => e.AppendAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWork.InSequence(sequence).Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var act = async () => await CreateHandler().Handle(new BlockchainPullCommand("eth-main"), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
