using TradeX.Domain.Abstractions.Interfaces;

namespace TradeX.Domain.Abstractions.Entities;

public class BaseEntity : IAuditableEntityWithSoftDelete, IBaseEntity
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? ModifiedByUserId { get; set; }
}

