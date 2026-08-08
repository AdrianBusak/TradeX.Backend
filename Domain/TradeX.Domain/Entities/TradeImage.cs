using TradeX.Domain.Abstractions.Entities;

namespace TradeX.Domain.Entities;

public class TradeImage : BaseEntity
{
    public Guid TradeId { get; set; }
    public Guid UserId { get; set; }
    public string BlobPath { get; set; } = null!;
    public string OriginalFileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
}
