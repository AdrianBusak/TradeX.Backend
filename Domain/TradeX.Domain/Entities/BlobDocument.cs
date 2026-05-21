using TradeX.Domain.Abstractions.Entities;

namespace TradeX.Domain.Entities;

public class BlobDocument : BaseEntity
{
    public string Name { get; set; } = null!;

    public string BlobPath { get; set; } = null!;
    public string? ThumbnailBlobPath { get; set; } = null!;

    // MIME tip (npr. "image/jpeg", "application/pdf")
    public string ContentType { get; set; } = null!;

    public long FileSize { get; set; }

    // Opcionalni hash za provjeru integriteta ili duplikata
    public string? ContentHash { get; set; }
}
