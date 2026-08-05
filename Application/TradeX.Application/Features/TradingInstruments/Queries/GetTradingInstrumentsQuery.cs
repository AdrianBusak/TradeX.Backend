using MediatR;
using TradeX.Application.Abstractions.Configuration;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Abstractions.QueryParameters;
using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.TradingInstruments.Queries;

public sealed class GetTradingInstrumentsQuery
    : ContextualRequest,
      IRequest<StandardListResponse<GetTradingInstrumentsQueryResponseModel>>,
      IAuthenticatedRequest
{
    public PagingQueryParameters? PagingParameters { get; set; }
    public FilterQueryParameters? FilterParameters { get; set; }
    public SortQueryParameters? SortParameters { get; set; }
}

public sealed class GetTradingInstrumentsQueryHandler(
    ITradeXRepository repository,
    ApplicationConfiguration configuration)
    : IRequestHandler<GetTradingInstrumentsQuery, StandardListResponse<GetTradingInstrumentsQueryResponseModel>>
{
    public async Task<StandardListResponse<GetTradingInstrumentsQueryResponseModel>> Handle(
        GetTradingInstrumentsQuery request,
        CancellationToken cancellationToken)
    {
        var query = GetQuery(request);

        var data = await repository
            .QueryAsync(
                query.OrderBySortParameters(GetSortParameters(request.SortParameters)),
                pageIndex: request.PagingParameters?.Index ?? 0,
                pageSize: request.PagingParameters?.Size
                    ?? configuration.DataRetrievalConfiguration?.DefaultPageSize
                    ?? 10,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new StandardListResponse<GetTradingInstrumentsQueryResponseModel>(
            data.Records!,
            data.TotalRecordCount,
            data.PageIndex,
            data.PageSize);
    }

    private IQueryable<GetTradingInstrumentsQueryResponseModel> GetQuery(
        GetTradingInstrumentsQuery request)
    {
        var query =
            from instrument in repository.DbContext.TradingInstrument
            where instrument.UserId == request.UserId()
            select new GetTradingInstrumentsQueryResponseModel
            {
                Id = instrument.Id,
                Symbol = instrument.Symbol,
                MarketType = instrument.MarketType,
                IsActive = instrument.IsActive,
                CreatedAt = instrument.CreatedAt,
                ModifiedAt = instrument.ModifiedAt
            };

        var filterParameters = request.FilterParameters;

        query = query
            .ApplyGuidFilter(filterParameters?.GetGuidFilter("id"), x => x.Id)
            .ApplyStringFilter(filterParameters?.GetStringFilter("symbol"), x => x.Symbol)
            .ApplyBoolFilter(filterParameters?.GetBoolFilter("isActive"), x => x.IsActive);

        var marketType = filterParameters?.GetStringFilter("marketType")?.Eq;

        if (Enum.TryParse<MarketType>(marketType, true, out var parsedMarketType))
        {
            query = query.Where(x => x.MarketType == parsedMarketType);
        }

        return query;
    }

    private static SortQueryParameters GetSortParameters(
        SortQueryParameters? sortParameters)
    {
        if (sortParameters?.Count > 0)
        {
            return sortParameters;
        }

        return
        [
            new SortQueryParameter(
                nameof(GetTradingInstrumentsQueryResponseModel.Symbol),
                SortDirection.Asc)
        ];
    }
}

public sealed class GetTradingInstrumentsQueryResponseModel
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = null!;
    public MarketType MarketType { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}
