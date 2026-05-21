namespace TradeX.Application.Abstractions.Interfaces;

public interface ICacheService
{
    Task<T> ExecuteAsync<T>(string policyName, string cacheKey, Func<Task<T>> action);
}
