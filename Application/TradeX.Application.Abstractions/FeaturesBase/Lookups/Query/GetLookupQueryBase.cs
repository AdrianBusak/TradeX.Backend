using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Configuration;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Abstractions.QueryParameters;
using MediatR;

namespace TradeX.Application.Abstractions.FeaturesBase.Lookups.Query;

public abstract class GetLookupQueryBase(FilterQueryParameters? filterParameters , PagingQueryParameters? pagingParameters, SortQueryParameters? sortParameters) : ContextualRequest, IRequest<StandardListResponse<LookupQueryResponseModel>>
{
    public PagingQueryParameters? PagingParameters { get; set; } = pagingParameters;
    public FilterQueryParameters? FilterParameters { get; set; } = filterParameters;
    public SortQueryParameters? SortParameters { get; set; } = sortParameters;
}

public  abstract class GetLookupHandlerBase<TQuery>(ITradeXRepository tradeXRepository, ApplicationConfiguration configuration) : IRequestHandler<TQuery, StandardListResponse<LookupQueryResponseModel>>
    where TQuery: GetLookupQueryBase, IRequest<StandardListResponse<LookupQueryResponseModel>>
{
    protected readonly ITradeXRepository _tradeXRepository = tradeXRepository;
    protected readonly ApplicationConfiguration _configuration = configuration;

    public abstract IQueryable<LookupQueryResponseModel> GetQueryInner(TQuery request);

    public async Task<StandardListResponse<LookupQueryResponseModel>> Handle(TQuery request, CancellationToken cancellationToken)
    {
        var query = GetQuery(request);
               
        cancellationToken.ThrowIfCancellationRequested();

        var data = await _tradeXRepository
                            .QueryAsync(query, pageIndex: request.PagingParameters?.Index ?? 0, pageSize: request.PagingParameters?.Size ?? _configuration.DataRetrievalConfiguration?.DefaultPageSize ?? 10, cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var response = new StandardListResponse<LookupQueryResponseModel>(data.Records!, data.TotalRecordCount, data.PageIndex, data.PageSize);

        return response;
    }

    
    private IQueryable<LookupQueryResponseModel> GetQuery(TQuery request)
    {
        var db = _tradeXRepository.DbContext;

        var query = GetQueryInner(request);

        var filterParameters = request.FilterParameters;

        query = query
            .ApplyGuidFilter(filterParameters?.GetGuidFilter("id"), x => x.Id)
            .ApplyStringFilter(filterParameters?.GetStringFilter("display"), x => x.Display)
            .ApplyBoolFilter(filterParameters?.GetBoolFilter("isActive"), x => x.IsActive);

        var sortParameters = request.SortParameters;

        if (!(sortParameters?.Count > 0))
        {
            sortParameters = [new SortQueryParameter("display", Enums.SortDirection.Asc)];
        }
        
        query = query.OrderBySortParameters(sortParameters);

        return query;
    }
}
