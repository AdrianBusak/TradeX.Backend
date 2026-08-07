using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.LotCalculator.Services;

public interface IInstrumentSpecificationResolver
{
    InstrumentSpecification GetIndexSpecification(string symbol);
}

public sealed class InstrumentSpecification
{
    public string Symbol { get; set; } = null!;
    public MarketType MarketType { get; set; }
    public string QuoteCurrency { get; set; } = null!;
    public decimal ContractSize { get; set; }
    public decimal PipSize { get; set; }
    public decimal LotStep { get; set; }
    public decimal MinLot { get; set; }
}
