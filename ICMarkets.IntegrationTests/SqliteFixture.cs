using ICMarkets.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ICMarkets.IntegrationTests;

public sealed class SqliteFixture : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private DbContextOptions<IcMarketsDbContext> _options = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        _options = new DbContextOptionsBuilder<IcMarketsDbContext>()
            .UseSqlite(_connection)
            .Options;

        await using var context = new IcMarketsDbContext(_options);
        await context.Database.EnsureCreatedAsync();
    }

    public IcMarketsDbContext CreateContext() => new(_options);

    public async Task DisposeAsync() => await _connection.DisposeAsync();
}

[CollectionDefinition(nameof(SqliteCollection))]
public sealed class SqliteCollection : ICollectionFixture<SqliteFixture>;
