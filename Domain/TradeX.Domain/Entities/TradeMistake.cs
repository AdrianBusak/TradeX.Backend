using TradeX.Domain.Abstractions.Entities;

namespace TradeX.Domain.Entities;

public partial class TradeMistake : BaseEntity
{
    public Guid TradeId { get; set; }
    public Guid MistakeId { get; set; }
    public string? Note { get; set; }
}
