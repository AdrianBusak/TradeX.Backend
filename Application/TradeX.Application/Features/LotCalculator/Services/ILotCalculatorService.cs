using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.LotCalculator.Services;

public interface ILotCalculatorService
{
    Task<LotCalculationResult> CalculateAsync(
        LotCalculationInput input,
        CancellationToken cancellationToken);
}

public sealed class LotCalculationInput
{
    public string Symbol { get; init; } = null!;
    public MarketType MarketType { get; init; }
    public string AccountCurrency { get; init; } = null!;
    public decimal AccountBalance { get; init; }
    public decimal RiskPercent { get; init; }
    public decimal? EntryPrice { get; init; }
    public decimal? StopLossPrice { get; init; }
    public decimal? StopLossPips { get; init; }
}

public sealed class LotCalculationResult
{
    public string Symbol { get; init; } = null!;
    public MarketType MarketType { get; init; }
    public decimal StopLossPips { get; init; }
    public decimal PipSize { get; init; }
    public decimal ContractSize { get; init; }
    public decimal PipValuePerLot { get; init; }
    public decimal LotSize { get; init; }
    public decimal RoundedLotSize { get; init; }
    public decimal EstimatedLoss { get; init; }
    public string? Warning { get; init; }
}
