using System.Collections.Specialized;
using AutoMapper;
using ICMarkets.Application.Abstractions;
using ICMarkets.Application.Abstractions.Repositories;
using ICMarkets.Infrastructure.BackgroundJobs;
using ICMarkets.Infrastructure.BlockChainClient;
using ICMarkets.Infrastructure.Options;
using ICMarkets.Infrastructure.Persistence;
using ICMarkets.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace ICMarkets.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //Options
        services.Configure<PollingOptions>(configuration.GetSection(PollingOptions.SectionName));
        services.Configure<BlockCypherOptions>(configuration.GetSection(BlockCypherOptions.SectionName));

        //Services
        services.AddHostedService<BlockchainPollingService>();
        services.AddSingleton<IClock, Clock.Clock>();

        //DB
        var connectionString = configuration.GetConnectionString("Default") ?? "Data Source=icmarkets.db";
        services.AddDbContext<IcMarketsDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        //Repositories
        services.AddScoped<IBlockchainSnapshotRepository, BlockchainSnapshotRepository>();
        services.AddScoped<IEventStoreRepository, EventStoreRepository>();

        //Http
        services.AddHttpClient<IBlockChainClient, BlockCypherClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BlockCypherOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddPolicyHandler((sp, _) =>
            {
                var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<BlockCypherOptions>>().Value;
                return HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(
                        Math.Max(0, options.RetryCount),
                        attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)));
            });
        
        services.AddSingleton<RateLimiter>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<BlockCypherOptions>>().Value;

            return new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = Math.Max(1, options.RateLimitTokenLimit),
                TokensPerPeriod = Math.Max(1, options.RateLimitTokensPerPeriod),
                ReplenishmentPeriod = TimeSpan.FromSeconds(
                    Math.Max(1, options.RateLimitReplenishmentSeconds)),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = Math.Max(0, options.RateLimitQueueLimit),
                AutoReplenishment = true
            });
        });

        //Mapper
        services.AddAutoMapper(
            _ => { },
            typeof(DependencyInjection).Assembly);
        return services;
    }
}
