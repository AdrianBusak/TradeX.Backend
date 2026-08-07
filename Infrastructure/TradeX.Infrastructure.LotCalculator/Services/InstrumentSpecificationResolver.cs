using Microsoft.Extensions.Configuration;
using TradeX.Application.Clients.Features.LotCalculator.Services;
using TradeX.Domain.Enums;

namespace TradeX.Infrastructure.LotCalculator.Services;

public sealed class InstrumentSpecificationResolver(IConfiguration configuration)
    : IInstrumentSpecificationResolver
{
    private readonly Dictionary<string, InstrumentSpecification> _specifications = configuration
        .GetSection("LotCalculator:IndexSpecifications").GetChildren()
        .Select(x => new InstrumentSpecification
        {
            Symbol = x["Symbol"] ?? string.Empty,
            MarketType = Enum.TryParse<MarketType>(x["MarketType"], true, out var marketType) ? marketType : default,
            QuoteCurrency = x["QuoteCurrency"] ?? string.Empty,
            ContractSize = decimal.TryParse(x["ContractSize"], out var contractSize) ? contractSize : 0,
            PipSize = decimal.TryParse(x["PipSize"], out var pipSize) ? pipSize : 0,
            LotStep = decimal.TryParse(x["LotStep"], out var lotStep) ? lotStep : 0,
            MinLot = decimal.TryParse(x["MinLot"], out var minLot) ? minLot : 0
        })
        .Where(x => x.MarketType == MarketType.Indices)
        .ToDictionary(x => NormalizeSymbol(x.Symbol), StringComparer.OrdinalIgnoreCase);

    public InstrumentSpecification GetIndexSpecification(string symbol)
    {
        var normalized = NormalizeSymbol(symbol);
        if (!_specifications.TryGetValue(normalized, out var specification) ||
            specification.ContractSize <= 0 || specification.PipSize <= 0 || specification.LotStep <= 0 ||
            specification.MinLot <= 0 || string.IsNullOrWhiteSpace(specification.QuoteCurrency))
        {
            throw new ArgumentException($"Unsupported index symbol '{normalized}'.");
        }

        return specification;
    }

    private static string NormalizeSymbol(string symbol) => 
        symbol.Trim()
        .ToUpperInvariant()
        .Replace("/", string.Empty)
        .Replace("-", string.Empty)
        .Replace("_", string.Empty);
}
