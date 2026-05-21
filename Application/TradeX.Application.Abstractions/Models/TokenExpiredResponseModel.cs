namespace TradeX.Application.Abstractions.Models;

public class TokenExpiredResponseModel
{
    public bool TokenExpired { get; set; } = true;
    public string? Message { get; set; }
}
