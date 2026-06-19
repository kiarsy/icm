using ICMarkets.Api.Controllers;
using ICMarkets.Application.Abstractions;
using ICMarkets.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ICMarkets.FunctionalTests;

public sealed class ApiFactory : WebApplicationFactory<BlockChainController>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"icmarkets_func_{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
        builder.UseSetting("Polling:Enabled", "false");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IBlockChainClient>();
            services.AddSingleton<IBlockChainClient, StubBlockChainClient>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<IcMarketsDbContext>().Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
        {
            return;
        }

        SqliteConnection.ClearAllPools();
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            try { File.Delete(_dbPath + suffix); }
            catch {  }
        }
    }
}
