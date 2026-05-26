using MediatR;
using TradeX.Application.Abstractions.Configuration;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Abstractions.QueryParameters;
using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.Strategies.Queries;

public sealed class GetStrategiesQuery
    : ContextualRequest,
      IRequest<StandardListResponse<GetStrategiesQueryResponseModel>>,
      IAuthenticatedRequest
{
    public PagingQueryParameters? PagingParameters { get; set; }
    public FilterQueryParameters? FilterParameters { get; set; }
    public SortQueryParameters? SortParameters { get; set; }
}

public sealed class GetStrategiesQueryHandler(
    ITradeXRepository repository,
    ApplicationConfiguration configuration)
    : IRequestHandler<GetStrategiesQuery, StandardListResponse<GetStrategiesQueryResponseModel>>
{
    public async Task<StandardListResponse<GetStrategiesQueryResponseModel>> Handle(
        GetStrategiesQuery request,
        CancellationToken cancellationToken)
    {
        var query = GetQuery(request);

        var data = await repository
            .QueryAsync(
                query,
                pageIndex: request.PagingParameters?.Index ?? 0,
                pageSize: request.PagingParameters?.Size
                    ?? configuration.DataRetrievalConfiguration?.DefaultPageSize
                    ?? 10,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new StandardListResponse<GetStrategiesQueryResponseModel>(
            data.Records!,
            data.TotalRecordCount,
            data.PageIndex,
            data.PageSize);
    }

    private IQueryable<GetStrategiesQueryResponseModel> GetQuery(GetStrategiesQuery request)
    {
        var query =
            from strategy in repository.DbContext.Strategy
            where strategy.UserId == request.UserId()
            select new GetStrategiesQueryResponseModel
            {
                Id = strategy.Id,
                Name = strategy.Name,
                Description = strategy.Description,
                MarketType = strategy.MarketType,
                Color = strategy.Color,
                IsActive = strategy.IsActive,
                CreatedAt = strategy.CreatedAt,
                ModifiedAt = strategy.ModifiedAt
            };

        var filterParameters = request.FilterParameters;

        query = ApplySearchFilter(query, filterParameters?.GetStringFilter("search"));

        query = query
            .ApplyGuidFilter(filterParameters?.GetGuidFilter("id"), x => x.Id)
            .ApplyStringFilter(filterParameters?.GetStringFilter("name"), x => x.Name)
            .ApplyStringFilter(filterParameters?.GetStringFilter("description"), x => x.Description)
            .ApplyStringFilter(filterParameters?.GetStringFilter("color"), x => x.Color)
            .ApplyBoolFilter(filterParameters?.GetBoolFilter("isActive"), x => x.IsActive);

        query = ApplyMarketTypeFilter(query, filterParameters?.GetStringFilter("marketType"));

        var sortParameters = request.SortParameters;

        if (!(sortParameters?.Count > 0))
        {
            sortParameters =
            [
                new SortQueryParameter(nameof(GetStrategiesQueryResponseModel.Name), SortDirection.Asc)
            ];
        }

        return query.OrderBySortParameters(sortParameters);
    }

    private static IQueryable<GetStrategiesQueryResponseModel> ApplySearchFilter(
        IQueryable<GetStrategiesQueryResponseModel> query,
        FilterQueryParameterDeconstructed<string?>? filter)
    {
        var search = NormalizeFilterValue(filter?.Contains ?? filter?.Eq ?? filter?.StartsWith);

        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        return query.Where(strategy =>
            strategy.Name.Contains(search) ||
            (strategy.Description != null && strategy.Description.Contains(search)));
    }

    private static IQueryable<GetStrategiesQueryResponseModel> ApplyMarketTypeFilter(
        IQueryable<GetStrategiesQueryResponseModel> query,
        FilterQueryParameterDeconstructed<string?>? filter)
    {
        if (filter is null)
        {
            return query;
        }

        if (TryParseMarketType(filter.Eq, out var eq))
        {
            query = query.Where(strategy => strategy.MarketType == eq);
        }

        if (TryParseMarketType(filter.Neq, out var neq))
        {
            query = query.Where(strategy => strategy.MarketType != neq);
        }

        return query;
    }

    private static bool TryParseMarketType(string? value, out MarketType marketType)
        => Enum.TryParse(NormalizeFilterValue(value), ignoreCase: true, out marketType);

    private static string? NormalizeFilterValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class GetStrategiesQueryResponseModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public MarketType MarketType { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}
