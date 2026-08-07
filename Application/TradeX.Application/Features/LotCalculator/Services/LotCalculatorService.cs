using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.LotCalculator.Services;

public sealed class LotCalculatorService(
    IExchangeRateProvider exchangeRateProvider,
    IInstrumentSpecificationResolver instrumentSpecificationResolver)
    : ILotCalculatorService
{
    public async Task<LotCalculationResult> CalculateAsync(
        LotCalculationInput input,
        CancellationToken cancellationToken)
    {
        var accountCurrency = NormalizeCurrency(input.AccountCurrency);
        var symbol = NormalizeSymbol(input.Symbol);
        var specification = GetSpecification(input.MarketType, symbol);
        var stopLossPips = GetStopLossPips(input, specification.PipSize);
        var conversionRate = await exchangeRateProvider
            .GetRateAsync(specification.QuoteCurrency, accountCurrency, cancellationToken)
            .ConfigureAwait(false);
        var pipValuePerLot = specification.ContractSize * specification.PipSize * conversionRate;
        var riskAmount = input.AccountBalance * input.RiskPercent / 100m;
        var lotSize = riskAmount / (stopLossPips * pipValuePerLot);

        if (lotSize <= 0 || pipValuePerLot <= 0)
        {
            throw new ArgumentException("Calculated lot size must be positive.");
        }

        var roundedLotSize = decimal.Floor(lotSize / specification.LotStep) * specification.LotStep;
        var warning = input.MarketType == MarketType.Indices
            ? "Index lot calculation uses configured contract specifications. Verify with your broker."
            : null;

        if (roundedLotSize < specification.MinLot)
        {
            warning = string.Join(" ", new[] { warning, $"Calculated lot size is below the minimum lot of {specification.MinLot}." }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        return new LotCalculationResult
        {
            Symbol = symbol,
            MarketType = input.MarketType,
            StopLossPips = stopLossPips,
            PipSize = specification.PipSize,
            ContractSize = specification.ContractSize,
            PipValuePerLot = pipValuePerLot,
            LotSize = lotSize,
            RoundedLotSize = roundedLotSize,
            EstimatedLoss = roundedLotSize * stopLossPips * pipValuePerLot,
            Warning = warning
        };
    }

    private InstrumentSpecification GetSpecification(MarketType marketType, string symbol)
    {
        if (marketType == MarketType.Indices)
        {
            return instrumentSpecificationResolver.GetIndexSpecification(symbol);
        }

        if (marketType != MarketType.Forex || symbol.Length != 6 || !symbol.All(char.IsLetter))
        {
            throw new ArgumentException("Only six-letter Forex symbols and configured Indices are supported.");
        }

        var quoteCurrency = symbol[3..];
        return new InstrumentSpecification
        {
            Symbol = symbol,
            MarketType = MarketType.Forex,
            QuoteCurrency = quoteCurrency,
            ContractSize = 100000m,
            PipSize = quoteCurrency == "JPY" ? 0.01m : 0.0001m,
            LotStep = 0.01m,
            MinLot = 0.01m
        };
    }

    private static decimal GetStopLossPips(LotCalculationInput input, decimal pipSize)
    {
        if (input.StopLossPips is > 0)
        {
            return input.StopLossPips.Value;
        }

        if (input.EntryPrice is not > 0 || input.StopLossPrice is not > 0 ||
            input.EntryPrice == input.StopLossPrice)
        {
            throw new ArgumentException("Provide StopLossPips, or distinct positive EntryPrice and StopLossPrice values.");
        }

        return decimal.Abs(input.EntryPrice.Value - input.StopLossPrice.Value) / pipSize;
    }

    private static string NormalizeSymbol(string symbol) => symbol.Trim().ToUpperInvariant()
        .Replace("/", string.Empty).Replace("-", string.Empty).Replace("_", string.Empty);

    private static string NormalizeCurrency(string currency)
    {
        var normalized = currency.Trim().ToUpperInvariant();
        if (normalized.Length != 3 || !normalized.All(char.IsLetter))
        {
            throw new ArgumentException("Account currency must be a three-letter ISO currency code.");
        }

        return normalized;
    }
}
