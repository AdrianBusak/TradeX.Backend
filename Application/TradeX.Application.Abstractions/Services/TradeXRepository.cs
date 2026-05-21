using TradeX.Application.Abstractions.Interfaces;
using TradeX.Repository;
using TradeX.Repository.Services;
using Microsoft.Extensions.Logging;

namespace TradeX.Application.Abstractions.Services;

public class TradeXRepository(TradeXDbContext dbContext, ILogger<RepositoryService<TradeXDbContext>> logger) : 
    RepositoryService<TradeXDbContext>(dbContext, logger), 
    ITradeXRepository
{
}
