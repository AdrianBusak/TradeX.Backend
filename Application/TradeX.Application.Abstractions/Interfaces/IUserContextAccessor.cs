namespace TradeX.Application.Abstractions.Interfaces;

using TradeX.Application.Abstractions.Models;

public interface IUserContextAccessor
{
    Task<AuthenticatedUserContext> GetAuthenticatedUserAsync();
}
