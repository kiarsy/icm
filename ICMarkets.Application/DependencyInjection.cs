using System.Reflection;
using FluentValidation;
using ICMarkets.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ICMarkets.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MediaRPipelineBehavior<,>));
        services.AddValidatorsFromAssembly(assembly);
        
        return services;
    }
}