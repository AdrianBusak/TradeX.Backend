using TradeX.Domain.Abstractions.Entities;
using TradeX.Domain.Enums;

namespace TradeX.Domain.Entities;

public partial class Trade : BaseEntity
{
    public Guid UserId { get; set; }

    public Guid StrategyId { get; set; }

    public Guid TradingInstrumentId { get; set; }

    public TradeDirection Direction { get; set; }

    public TradingSession? Session { get; set; }

    public TradeStatus Status { get; set; } = TradeStatus.Planned;

    public DateTime TradeDate { get; set; }

    public decimal? EntryPrice { get; set; }

    public decimal? ExitPrice { get; set; }

    public decimal? StopLoss { get; set; }

    public decimal? TakeProfit { get; set; }

    public decimal? LotSize { get; set; }

    public decimal? RiskAmount { get; set; }

    public decimal? PnL { get; set; }

    public decimal? RMultiple { get; set; }

    public string? Notes { get; set; }

    public ICollection<TradeAccountAssignment> AccountAssignments { get; set; } =
        new List<TradeAccountAssignment>();
}
