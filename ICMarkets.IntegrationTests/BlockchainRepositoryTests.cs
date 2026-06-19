using FluentAssertions;
using ICMarkets.Domain;
using ICMarkets.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ICMarkets.IntegrationTests;

[Collection(nameof(SqliteCollection))]
public class BlockchainRepositoryTests(SqliteFixture fixture)
{
    private static BlockchainModel Model(string identifier, long height) => new()
    {
        Id = Guid.NewGuid(),
        BlockchainIdentifier = identifier,
        CreatedAt = DateTime.UtcNow,
        Name = identifier,
        Height = height,
        Hash = $"hash-{height}"
    };

    private async Task CaptureAsync(BlockchainModel model)
    {
        // Mirrors a single capture: a fresh context (unit of work) per upsert.
        await using var context = fixture.CreateContext();
        await new BlockchainRepository(context).AddAsync(model, CancellationToken.None);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task AddAsync_inserts_a_new_chain_then_upserts_in_place_and_bumps_revision()
    {
        const string id = "repo-btc-main";

        await CaptureAsync(Model(id, 100));
        await CaptureAsync(Model(id, 101)); // same identifier -> update existing row

        await using var context = fixture.CreateContext();
        var rows = await context.BlockChain.Where(b => b.BlockchainIdentifier == id).ToListAsync();

        rows.Should().HaveCount(1);                 // upserted, not duplicated
        rows[0].Height.Should().Be(101);            // latest values applied
        context.Entry(rows[0]).Property("Revision").CurrentValue.Should().Be(2);
    }

    [Fact]
    public async Task GetLatest_returns_all_or_filters_by_identifier()
    {
        await CaptureAsync(Model("repo-eth-main", 1));
        await CaptureAsync(Model("repo-ltc-main", 2));

        await using var context = fixture.CreateContext();
        var repo = new BlockchainRepository(context);

        (await repo.GetLatest(null)).Select(b => b.BlockchainIdentifier)
            .Should().Contain(new[] { "repo-eth-main", "repo-ltc-main" });

        var filtered = await repo.GetLatest("repo-eth-main");
        filtered.Should().ContainSingle().Which.BlockchainIdentifier.Should().Be("repo-eth-main");
    }
}
