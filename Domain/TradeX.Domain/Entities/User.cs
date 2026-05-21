using TradeX.Domain.Abstractions.Entities;

namespace TradeX.Domain.Entities;

public partial class User : BaseEntity
{
    public string ExternalId { get; set; } = null!;
}
