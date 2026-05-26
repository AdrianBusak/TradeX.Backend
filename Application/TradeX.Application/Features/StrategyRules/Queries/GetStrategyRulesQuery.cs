using MediatR;
using TradeX.Application.Abstractions.Configuration;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Abstractions.QueryParameters;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.StrategyRules.Queries;

public sealed class GetStrategyRulesQuery(Guid strategyId)
    : ContextualRequest,
      IRequest<StandardListResponse<GetStrategyRulesQueryResponseModel>>,
      IAuthenticatedRequest
{
    public Guid StrategyId { get; } = strategyId;
    public PagingQueryParameters? PagingParameters { get; set; }
    public FilterQueryParameters? FilterParameters { get; set; }
    public SortQueryParameters? SortParameters { get; set; }
}

public sealed class GetStrategyRulesQueryHandler(
    ITradeXRepository repository,
    ApplicationConfiguration configuration)
    : IRequestHandler<GetStrategyRulesQuery, StandardListResponse<GetStrategyRulesQueryResponseModel>>
{
    public async Task<StandardListResponse<GetStrategyRulesQueryResponseModel>> Handle(
        GetStrategyRulesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        if (!await StrategyExistsAsync(request.StrategyId, userId, cancellationToken).ConfigureAwait(false))
        {
            return new StandardListResponse<GetStrategyRulesQueryResponseModel>(
                OperationResult.NotFound,
                $"Entity with the given key not found. [Id: {request.StrategyId}]] [EntityType: {nameof(Strategy)}]",
                null!);
        }

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

        return new StandardListResponse<GetStrategyRulesQueryResponseModel>(
            data.Records!,
            data.TotalRecordCount,
            data.PageIndex,
            data.PageSize);
    }

    private IQueryable<GetStrategyRulesQueryResponseModel> GetQuery(GetStrategyRulesQuery request)
    {
        var query =
            from rule in repository.DbContext.StrategyRule
            where rule.StrategyId == request.StrategyId
            select new GetStrategyRulesQueryResponseModel
            {
                Id = rule.Id,
                Title = rule.Title,
                Description = rule.Description,
                Order = rule.Order,
                IsRequired = rule.IsRequired,
                Category = rule.Category,
                Importance = rule.Importance,
                IsActive = rule.IsActive,
                CreatedAt = rule.CreatedAt,
                ModifiedAt = rule.ModifiedAt
            };

        var filterParameters = request.FilterParameters;

        query = ApplySearchFilter(query, filterParameters?.GetStringFilter("search"));

        query = query
            .ApplyGuidFilter(filterParameters?.GetGuidFilter("id"), x => x.Id)
            .ApplyStringFilter(filterParameters?.GetStringFilter("title"), x => x.Title)
            .ApplyStringFilter(filterParameters?.GetStringFilter("description"), x => x.Description)
            .ApplyIntFilter(filterParameters?.GetIntFilter("order"), x => x.Order)
            .ApplyBoolFilter(filterParameters?.GetBoolFilter("isRequired"), x => x.IsRequired)
            .ApplyBoolFilter(filterParameters?.GetBoolFilter("isActive"), x => x.IsActive);

        query = ApplyCategoryFilter(query, filterParameters?.GetStringFilter("category"));
        query = ApplyImportanceFilter(query, filterParameters?.GetStringFilter("importance"));

        var sortParameters = request.SortParameters;

        if (!(sortParameters?.Count > 0))
        {
            sortParameters =
            [
                new SortQueryParameter(nameof(GetStrategyRulesQueryResponseModel.Order), SortDirection.Asc)
            ];
        }

        return query.OrderBySortParameters(sortParameters);
    }

    private async Task<bool> StrategyExistsAsync(
        Guid strategyId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var id = await repository.GetIdAsync<Strategy>(
                strategy => strategy.Id == strategyId && strategy.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        return id.HasValue;
    }

    private static IQueryable<GetStrategyRulesQueryResponseModel> ApplySearchFilter(
        IQueryable<GetStrategyRulesQueryResponseModel> query,
        FilterQueryParameterDeconstructed<string?>? filter)
    {
        var search = NormalizeFilterValue(filter?.Contains ?? filter?.Eq ?? filter?.StartsWith);

        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        return query.Where(rule =>
            rule.Title.Contains(search) ||
            (rule.Description != null && rule.Description.Contains(search)));
    }

    private static string? NormalizeFilterValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static IQueryable<GetStrategyRulesQueryResponseModel> ApplyCategoryFilter(
        IQueryable<GetStrategyRulesQueryResponseModel> query,
        FilterQueryParameterDeconstructed<string?>? filter)
    {
        if (filter is null)
        {
            return query;
        }

        if (TryParseStrategyRuleCategory(filter.Eq, out var eq))
        {
            query = query.Where(rule => rule.Category == eq);
        }

        if (TryParseStrategyRuleCategory(filter.Neq, out var neq))
        {
            query = query.Where(rule => rule.Category != neq);
        }

        return query;
    }

    private static IQueryable<GetStrategyRulesQueryResponseModel> ApplyImportanceFilter(
        IQueryable<GetStrategyRulesQueryResponseModel> query,
        FilterQueryParameterDeconstructed<string?>? filter)
    {
        if (filter is null)
        {
            return query;
        }

        if (TryParseStrategyRuleImportance(filter.Eq, out var eq))
        {
            query = query.Where(rule => rule.Importance == eq);
        }

        if (TryParseStrategyRuleImportance(filter.Neq, out var neq))
        {
            query = query.Where(rule => rule.Importance != neq);
        }

        return query;
    }

    private static bool TryParseStrategyRuleCategory(string? value, out StrategyRuleCategory category)
        => Enum.TryParse(NormalizeFilterValue(value), ignoreCase: true, out category);

    private static bool TryParseStrategyRuleImportance(string? value, out StrategyRuleImportance importance)
        => Enum.TryParse(NormalizeFilterValue(value), ignoreCase: true, out importance);
}

public sealed class GetStrategyRulesQueryResponseModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public StrategyRuleCategory Category { get; set; }
    public StrategyRuleImportance Importance { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}
