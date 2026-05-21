using TradeX.Application.Abstractions.Interfaces;

namespace TradeX.Application.Clients.Tests.Services;

public class UserContextAccessor : IUserContextAccessor
{
    public async Task<Tuple<string?, bool>> GetUserIdentifierAsync()
    {
        return await Task.FromResult(new Tuple<string?, bool>("idp|12345", true));
    }
}
