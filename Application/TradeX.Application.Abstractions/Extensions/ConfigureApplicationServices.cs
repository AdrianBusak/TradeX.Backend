using FluentValidation;
using TradeX.Application.Abstractions.Behaviors;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Services;
using TradeX.Repository;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using Polly.Registry;
using System.Globalization;
using System.Reflection;

namespace TradeX.Application.Abstractions.Extensions;

public static class ConfigureApplicationServices
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration, Assembly applicationAssembly, Action<IServiceCollection>? mediatrAddingBehaviors = null, Action<IServiceCollection>? mediatrAddedBehaviors = null)
    {
        services.AddLogging();
        services.AddRepository(configuration.GetConnectionString("Db")!);

        services.AddScoped<ITradeXRepository, TradeXRepository>();

        services.AddMediatR(applicationAssembly, mediatrAddingBehaviors, mediatrAddedBehaviors);
        services.AddFluentValidation(applicationAssembly);

        services.AddPollyCaching();

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        return services;
    }

    private static IServiceCollection AddRepository(this IServiceCollection services, string dbConnectionString)
    {
        if (string.IsNullOrWhiteSpace(dbConnectionString))
        {
            throw new ArgumentException("DbConnectionString not provided", nameof(dbConnectionString));
        }

        services.AddDbContext<TradeXDbContext>(options =>
        
            options.UseSqlServer(dbConnectionString, o =>
            {
                o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                o.CommandTimeout(120);
            }),
            contextLifetime: ServiceLifetime.Scoped,
            optionsLifetime: ServiceLifetime.Singleton);

        // DbContext factory for singletons
        services.AddPooledDbContextFactory<TradeXDbContext>(opt =>
            opt.UseSqlServer(dbConnectionString, o=>
            {
                o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                o.CommandTimeout(120);
            }));

        return services;
    }

    private static IServiceCollection AddMediatR(this IServiceCollection services, Assembly applicationAssembly, Action<IServiceCollection>? mediatrAddingBehaviors = null, Action<IServiceCollection>? mediatrAddedBehaviors = null)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssemblies(applicationAssembly));
        services.AddFluentValidation(applicationAssembly);

        mediatrAddingBehaviors?.Invoke(services);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionsBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestAuthenticationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));

        mediatrAddedBehaviors?.Invoke(services);

        return services;
    }

    private static IServiceCollection AddFluentValidation(this IServiceCollection services, Assembly applicationAssembly)
    {
        services.AddValidatorsFromAssembly(applicationAssembly);

        ValidatorOptions.Global.LanguageManager.Enabled = false;
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en");

        return services;
    }
    private static IServiceCollection AddPollyCaching(this IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddSingleton<IAsyncCacheProvider, MemoryCacheProvider>();

        services.AddSingleton<IPolicyRegistry<string>>(sp =>
        {
            var registry = new PolicyRegistry();
            var memoryCache = sp.GetRequiredService<IMemoryCache>();
            var cacheProvider = new MemoryCacheProvider(memoryCache);

            registry.Add(Constants.CachePolicies.Cache30Min,
                Policy.CacheAsync(
                    cacheProvider,
                    TimeSpan.FromMinutes(30)));

            registry.Add(Constants.CachePolicies.Cache5Min,
                Policy.CacheAsync(
                    cacheProvider,
                    TimeSpan.FromMinutes(5)));

            registry.Add(Constants.CachePolicies.Cache1Min,
                Policy.CacheAsync(
                    cacheProvider,
                    TimeSpan.FromMinutes(1)));

            return registry;
        });

        services.AddScoped<ICacheService, CacheService>();

        return services;
    }

}
