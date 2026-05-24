using MediatR;
using TradeX.Application.Abstractions.Configuration;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Abstractions.QueryParameters;
using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.TradingAccounts.Queries;

public sealed class GetTradingAccountsQuery
    : ContextualRequest,
      IRequest<StandardListResponse<GetTradingAccountsQueryResponseModel>>,
      IAuthenticatedRequest
{
    public PagingQueryParameters? PagingParameters { get; set; }
    public FilterQueryParameters? FilterParameters { get; set; }
    public SortQueryParameters? SortParameters { get; set; }
}

public sealed class GetTradingAccountsQueryHandler(
    ITradeXRepository repository,
    ApplicationConfiguration configuration)
    : IRequestHandler<GetTradingAccountsQuery, StandardListResponse<GetTradingAccountsQueryResponseModel>>
{
    public async Task<StandardListResponse<GetTradingAccountsQueryResponseModel>> Handle(
        GetTradingAccountsQuery request,
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

        return new StandardListResponse<GetTradingAccountsQueryResponseModel>(
            data.Records!,
            data.TotalRecordCount,
            data.PageIndex,
            data.PageSize);
    }

    private IQueryable<GetTradingAccountsQueryResponseModel> GetQuery(GetTradingAccountsQuery request)
    {
        var query =
            from account in repository.DbContext.TradingAccount
            where account.UserId == request.UserId()
            select new GetTradingAccountsQueryResponseModel
            {
                Id = account.Id,
                Name = account.Name,
                AccountType = account.AccountType,
                Broker = account.Broker,
                Currency = account.Currency,
                InitialBalance = account.InitialBalance,
                CurrentBalance = account.CurrentBalance,
                IsActive = account.IsActive,
                CreatedAt = account.CreatedAt,
                ModifiedAt = account.ModifiedAt
            };

        var filterParameters = request.FilterParameters;

        query = ApplySearchFilter(query, filterParameters?.GetStringFilter("search"));

        query = query
            .ApplyGuidFilter(filterParameters?.GetGuidFilter("id"), x => x.Id)
            .ApplyStringFilter(filterParameters?.GetStringFilter("name"), x => x.Name)
            .ApplyStringFilter(filterParameters?.GetStringFilter("broker"), x => x.Broker)
            .ApplyStringFilter(filterParameters?.GetStringFilter("currency"), x => x.Currency)
            .ApplyBoolFilter(filterParameters?.GetBoolFilter("isActive"), x => x.IsActive);

        query = ApplyAccountTypeFilter(query, filterParameters?.GetStringFilter("accountType"));

        var sortParameters = request.SortParameters;

        if (!(sortParameters?.Count > 0))
        {
            sortParameters =
            [
                new SortQueryParameter(nameof(GetTradingAccountsQueryResponseModel.Name), SortDirection.Asc)
            ];
        }

        return query.OrderBySortParameters(sortParameters);
    }

    private static IQueryable<GetTradingAccountsQueryResponseModel> ApplySearchFilter(
        IQueryable<GetTradingAccountsQueryResponseModel> query,
        FilterQueryParameterDeconstructed<string?>? filter)
    {
        var search = NormalizeFilterValue(filter?.Contains ?? filter?.Eq ?? filter?.StartsWith);

        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        return query.Where(account =>
            account.Name.Contains(search) ||
            account.Broker.Contains(search) ||
            account.Currency.Contains(search));
    }

    private static IQueryable<GetTradingAccountsQueryResponseModel> ApplyAccountTypeFilter(
        IQueryable<GetTradingAccountsQueryResponseModel> query,
        FilterQueryParameterDeconstructed<string?>? filter)
    {
        if (filter is null)
        {
            return query;
        }

        if (TryParseAccountType(filter.Eq, out var eq))
        {
            query = query.Where(account => account.AccountType == eq);
        }

        if (TryParseAccountType(filter.Neq, out var neq))
        {
            query = query.Where(account => account.AccountType != neq);
        }

        return query;
    }

    private static bool TryParseAccountType(string? value, out TradingAccountType accountType)
        => Enum.TryParse(NormalizeFilterValue(value), ignoreCase: true, out accountType);

    private static string? NormalizeFilterValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class GetTradingAccountsQueryResponseModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public TradingAccountType AccountType { get; set; }
    public string Broker { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public decimal InitialBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}
