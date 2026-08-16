using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Abstractions.QueryParameters;
using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.EconomicCalendar.Queries;

public sealed class GetEconomicEventsQuery
    : ContextualRequest,
      IRequest<StandardResponse<GetEconomicEventsResponse>>,
      IAuthenticatedRequest
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }
    public FilterQueryParameters? FilterParameters { get; set; }

    public sealed class Validator : AbstractValidator<GetEconomicEventsQuery>
    {
        public Validator()
        {
            RuleFor(x => x.From).NotNull();
            RuleFor(x => x.To).NotNull();
            RuleFor(x => x)
                .Must(x => !x.From.HasValue || !x.To.HasValue || x.From < x.To)
                .WithMessage("From must be before To.");
            RuleFor(x => x)
                .Must(x => !x.From.HasValue || !x.To.HasValue || x.To - x.From <= TimeSpan.FromDays(31))
                .WithMessage("The requested range cannot exceed 31 days.");
        }
    }
}

public sealed class GetEconomicEventsQueryHandler(ITradeXRepository repository)
    : IRequestHandler<GetEconomicEventsQuery, StandardResponse<GetEconomicEventsResponse>>
{
    public async Task<StandardResponse<GetEconomicEventsResponse>> Handle(
        GetEconomicEventsQuery request,
        CancellationToken cancellationToken)
    {
        var items = await GetQuery(request)
            .AsNoTracking()
            .OrderBy(x => x.ScheduledAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var lastSyncedAt = await repository.DbContext.EconomicEvent
            .AsNoTracking()
            .MaxAsync(x => (DateTimeOffset?)x.LastSyncedAt, cancellationToken)
            .ConfigureAwait(false);

        return new StandardResponse<GetEconomicEventsResponse>(
            OperationResult.Ok,
            new GetEconomicEventsResponse { Items = items, LastSyncedAt = lastSyncedAt });
    }

    private IQueryable<EconomicEventResponse> GetQuery(GetEconomicEventsQuery request)
    {
        var from = request.From!.Value;
        var to = request.To!.Value;
        var filters = request.FilterParameters;

        var query = repository.DbContext.EconomicEvent
            .Where(x => x.ScheduledAt >= from && x.ScheduledAt < to)
            .Select(x => new EconomicEventResponse
            {
                Id = x.Id,
                Title = x.Title,
                Currency = x.Currency,
                ScheduledAt = x.ScheduledAt,
                Impact = x.Impact,
                Forecast = x.Forecast,
                Previous = x.Previous
            })
            .ApplyStringFilter(filters?.GetStringFilter("currency"), x => x.Currency)
            .ApplyStringFilter(filters?.GetStringFilter("title"), x => x.Title);

        if (Enum.TryParse<EconomicImpact>(
                filters?.GetStringFilter("impact")?.Eq,
                true,
                out var impact))
        {
            query = query.Where(x => x.Impact == impact);
        }

        return query;
    }
}

public sealed class GetEconomicEventsResponse
{
    public List<EconomicEventResponse> Items { get; set; } = [];
    public DateTimeOffset? LastSyncedAt { get; set; }
}

public sealed class EconomicEventResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public DateTimeOffset ScheduledAt { get; set; }
    public EconomicImpact Impact { get; set; }
    public string? Forecast { get; set; }
    public string? Previous { get; set; }
}
