namespace TradeX.Application.Abstractions.Models;

public sealed record AuthenticatedUserContext(
    string? ExternalUserId,
    string? Email,
    string? FirstName,
    string? LastName,
    bool IsActive);
