namespace TradeX.Application.Clients.Features.LotCalculator.Services;

public interface IExchangeRateProvider
{
    Task<decimal> GetRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken);
}
