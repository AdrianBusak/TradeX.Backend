using TradeX.Domain.Abstractions.Entities;

namespace TradeX.Domain.Entities;

public partial class User : BaseEntity
{
    public string ExternalId { get; set; } = null!;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
