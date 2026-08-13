using TradeX.Domain.Abstractions.Entities;

namespace TradeX.Domain.Entities;

public partial class Mistake : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ICollection<TradeMistake> TradeMistakes { get; set; } = new List<TradeMistake>();
}
