namespace TradeX.Application.Clients.Features.Trades;

public sealed class TradeImageResponseModel
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public string Url { get; set; } = null!;
}
