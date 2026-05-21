namespace TradeX.Domain.Abstractions.Interfaces;

public interface IBaseEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}
