using System.Collections.Specialized;
using AutoMapper;
using ICMarkets.Application.Abstractions;
using ICMarkets.Infrastructure.BackgroundJobs;
using ICMarkets.Infrastructure.BlockChainClient;
using ICMarkets.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace ICMarkets.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PollingOptions>(configuration.GetSection(PollingOptions.SectionName));
        services.Configure<BlockCypherOptions>(configuration.GetSection(BlockCypherOptions.SectionName));
        services.AddHostedService<BlockchainPollingService>();
        services.AddSingleton<IClock, Clock.Clock>();

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

        services.AddAutoMapper(
            _ => { },
            typeof(DependencyInjection).Assembly);
        return services;
    }
}