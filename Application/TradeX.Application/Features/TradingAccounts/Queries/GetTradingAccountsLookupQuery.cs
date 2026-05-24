using TradeX.Application.Abstractions.Configuration;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.FeaturesBase.Lookups.Query;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Abstractions.QueryParameters;

namespace TradeX.Application.Clients.Features.TradingAccounts.Queries;

public sealed class GetTradingAccountsLookupQuery(
    FilterQueryParameters? filterParameters,
    PagingQueryParameters? pagingParameters,
    SortQueryParameters? sortParameters)
    : GetLookupQueryBase(filterParameters, pagingParameters, sortParameters),
      IAuthenticatedRequest
{
}

public sealed class GetTradingAccountsLookupQueryHandler(
    ITradeXRepository repository,
    ApplicationConfiguration configuration)
    : GetLookupHandlerBase<GetTradingAccountsLookupQuery>(repository, configuration)
{
    public override IQueryable<LookupQueryResponseModel> GetQueryInner(GetTradingAccountsLookupQuery request)
    {
        return
            from account in _tradeXRepository.DbContext.TradingAccount
            where account.UserId == request.UserId()
            select new LookupQueryResponseModel
            {
                Id = account.Id,
                Display = account.Name,
                IsActive = account.IsActive
            };
    }
}
