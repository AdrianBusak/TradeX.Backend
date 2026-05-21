namespace TradeX.Application.Abstractions.Models;

public class AnonymousUserLoginResponse(string sessionId)
{
    public string? UserIdentifier { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? Message { get; set; }
    public string SessionId { get; set; } = sessionId;
}
