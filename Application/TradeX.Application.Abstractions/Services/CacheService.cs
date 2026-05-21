using TradeX.Application.Abstractions.Interfaces;
using Polly;
using Polly.Registry;

namespace TradeX.Application.Abstractions.Services;

public class CacheService : ICacheService
{
    private readonly IPolicyRegistry<string> _registry;

    public CacheService(IPolicyRegistry<string> registry)
    {
        _registry = registry;
    }

    public Task<T> ExecuteAsync<T>(string policyName, string cacheKey, Func<Task<T>> action)
    {
        var policy = _registry.Get<IAsyncPolicy>(policyName);

        return policy.ExecuteAsync(
            _ => action(),
            new Context(cacheKey));
    }
}
