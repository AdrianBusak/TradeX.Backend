namespace TradeX.Application.Clients.Features.Mistakes;

public sealed class MistakeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
