using FluentAssertions;
using ICMarkets.Domain;
using ICMarkets.Domain.Common.Exceptions;
using ICMarkets.Infrastructure.Persistence;
using ICMarkets.Infrastructure.Repositories;

namespace ICMarkets.IntegrationTests;

[Collection(nameof(SqliteCollection))]
public class EventStoreAndUnitOfWorkTests(SqliteFixture fixture)
{
    private static BlockchainSnapshotCaptured Event(string identifier, DateTime occurredAt, long height) =>
        new(new BlockchainModel
        {
            Id = Guid.NewGuid(),
            BlockchainIdentifier = identifier,
            CreatedAt = occurredAt,
            Name = identifier,
            Height = height,
            Hash = $"hash-{height}"
        });

    private async Task AppendAsync(BlockchainSnapshotCaptured @event)
    {
        await using var context = fixture.CreateContext();
        await new EventStoreRepository(context).AppendAsync(@event, CancellationToken.None);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task History_is_newest_first_round_trips_the_polymorphic_payload_and_counts()
    {
        const string id = "evt-eth-main";
        var t = new DateTime(2026, 6, 18, 8, 0, 0, DateTimeKind.Utc);

        await AppendAsync(Event(id, t, 10));
        await AppendAsync(Event(id, t.AddMinutes(1), 11));
        await AppendAsync(Event(id, t.AddMinutes(2), 12));

        await using var context = fixture.CreateContext();
        var repo = new EventStoreRepository(context);

        var history = await repo.GetAllHistoryAsync(id, page: 1, pageSize: 10, CancellationToken.None);

        history.Should().AllBeOfType<BlockchainSnapshotCaptured>();          // polymorphic deserialization
        history.Cast<BlockchainSnapshotCaptured>().Select(e => e.Model.Height)
            .Should().ContainInOrder(12L, 11L, 10L);                         // newest first
        (await repo.CountAsync(id, CancellationToken.None)).Should().Be(3);
    }

    [Fact]
    public async Task History_filters_by_identifier()
    {
        var t = new DateTime(2026, 6, 18, 9, 0, 0, DateTimeKind.Utc);
        await AppendAsync(Event("evt-dash-main", t, 1));
        await AppendAsync(Event("evt-btc-test3", t, 2));

        await using var context = fixture.CreateContext();
        var repo = new EventStoreRepository(context);

        var dash = await repo.GetAllHistoryAsync("evt-dash-main", 1, 10, CancellationToken.None);
        dash.Should().ContainSingle()
            .Which.Should().BeOfType<BlockchainSnapshotCaptured>()
            .Which.EventId.Should().Be("evt-dash-main");
    }

    [Fact]
    public async Task UnitOfWork_commit_persists_and_rollback_on_dispose_without_commit_persists_nothing()
    {
        var t = new DateTime(2026, 6, 18, 10, 0, 0, DateTimeKind.Utc);

        // Committed unit of work persists.
        await using (var context = fixture.CreateContext())
        {
            var uow = new UnitOfWork(context);
            await uow.BeginAsync();
            await new EventStoreRepository(context).AppendAsync(Event("uow-ltc-main", t, 100), CancellationToken.None);
            await uow.CommitAsync();
            await uow.DisposeAsync();
        }

        // Uncommitted unit of work rolls back on dispose.
        await using (var context = fixture.CreateContext())
        {
            var uow = new UnitOfWork(context);
            await uow.BeginAsync();
            await new EventStoreRepository(context).AppendAsync(Event("uow-ltc-main", t.AddMinutes(1), 200), CancellationToken.None);
            await uow.DisposeAsync(); // no commit
        }

        await using var verify = fixture.CreateContext();
        var count = await new EventStoreRepository(verify).CountAsync("uow-ltc-main", CancellationToken.None);
        count.Should().Be(1); // only the committed one
    }

    [Fact]
    public async Task UnitOfWork_maps_unique_violation_to_ConcurrentException()
    {
        var t = new DateTime(2026, 6, 18, 11, 0, 0, DateTimeKind.Utc);

        await using var context = fixture.CreateContext();
        var uow = new UnitOfWork(context);
        var repo = new EventStoreRepository(context);

        await uow.BeginAsync();
        // Same (EventId, OccurredAt) twice violates the unique index on the events table.
        await repo.AppendAsync(Event("conflict-chain", t, 1), CancellationToken.None);
        await repo.AppendAsync(Event("conflict-chain", t, 2), CancellationToken.None);

        var act = async () => await uow.CommitAsync();

        await act.Should().ThrowAsync<ConcurrentException>();
        await uow.DisposeAsync();
    }
}
