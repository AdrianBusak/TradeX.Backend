using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;

namespace TradeX.Application.Clients.Tests.Services;

public class UserContextAccessor : IUserContextAccessor
{
    public async Task<AuthenticatedUserContext> GetAuthenticatedUserAsync()
    {
        return await Task.FromResult(new AuthenticatedUserContext(
            "idp|12345",
            "user@example.com",
            "Test",
            "User",
            true));
    }
}
