namespace TradeX.Application.Abstractions.Interfaces;

public interface IUserContextAccessor
{
    Task<Tuple<string?, bool>> GetUserIdentifierAsync();
}
