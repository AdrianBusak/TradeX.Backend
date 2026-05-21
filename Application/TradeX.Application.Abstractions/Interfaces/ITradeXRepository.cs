using TradeX.Repository;
using TradeX.Repository.Abstractions.Interfaces;

namespace TradeX.Application.Abstractions.Interfaces;

public interface ITradeXRepository: IRepository<TradeXDbContext>
{
}
