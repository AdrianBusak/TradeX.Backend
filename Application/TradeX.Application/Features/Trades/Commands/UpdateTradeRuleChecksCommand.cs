using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Clients.Features.Trades;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Trades.Commands;

public sealed class UpdateTradeRuleChecksRequest
{
    public List<UpdateTradeRuleCheckItemRequest> Rules { get; set; } = [];
}

public sealed class UpdateTradeRuleCheckItemRequest
{
    public Guid StrategyRuleId { get; set; }
    public bool IsFollowed { get; set; }
    public string? Note { get; set; }
}

public sealed class UpdateTradeRuleChecksCommand(Guid tradeId, UpdateTradeRuleChecksRequest data)
    : BaseInput<UpdateTradeRuleChecksRequest>(data),
      IRequest<StandardResponse<GetTradeRuleChecksResponse>>,
      IAuthenticatedRequest
{
    public Guid TradeId { get; } = tradeId;

    public sealed class Validator : AbstractValidator<UpdateTradeRuleChecksCommand>
    {
        public Validator()
        {
            RuleFor(x => x.TradeId).NotEmpty();
            RuleFor(x => x.Model).NotNull().SetValidator(new RequestValidator());
        }
    }

    public sealed class RequestValidator : AbstractValidator<UpdateTradeRuleChecksRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Rules).NotNull();
            RuleFor(x => x.Rules)
                .Must(rules => rules is null || rules.Select(x => x.StrategyRuleId).Distinct().Count() == rules.Count)
                .WithMessage("Each strategy rule can be checked only once.");
            RuleForEach(x => x.Rules).SetValidator(new ItemValidator());
        }
    }

    public sealed class ItemValidator : AbstractValidator<UpdateTradeRuleCheckItemRequest>
    {
        public ItemValidator()
        {
            RuleFor(x => x.StrategyRuleId).NotEmpty();
            RuleFor(x => x.Note).MaximumLength(1000);
        }
    }
}

public sealed class UpdateTradeRuleChecksCommandHandler(ITradeXRepository repository)
    : IRequestHandler<UpdateTradeRuleChecksCommand, StandardResponse<GetTradeRuleChecksResponse>>
{
    public async Task<StandardResponse<GetTradeRuleChecksResponse>> Handle(
        UpdateTradeRuleChecksCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var trade = await repository.GetSingleAsync<Trade>(
                entity => entity.Id == request.TradeId && entity.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (trade is null)
        {
            return new StandardResponse<GetTradeRuleChecksResponse>(
                OperationResult.NotFound,
                "Trade was not found.",
                null!);
        }

        var items = request.Model.Rules;
        var ruleIds = items.Select(x => x.StrategyRuleId).Distinct().ToList();
        var validRuleIds = await repository.QueryAsync(
                from rule in repository.DbContext.StrategyRule
                where rule.StrategyId == trade.StrategyId && rule.IsActive && ruleIds.Contains(rule.Id)
                select new RuleIdRow { Id = rule.Id },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if ((validRuleIds.Records?.Count ?? 0) != ruleIds.Count)
        {
            return new StandardResponse<GetTradeRuleChecksResponse>(
                OperationResult.NotFound,
                "One or more strategy rules were not found for this trade's strategy.",
                null!);
        }

        var existingChecks = ruleIds.Count == 0
            ? []
            : await repository.GetListAsync<TradeRuleCheck>(
                    cancellationToken,
                    check => check.TradeId == trade.Id &&
                             check.UserId == userId &&
                             check.IsActive &&
                             ruleIds.Contains(check.StrategyRuleId))
                .ConfigureAwait(false);

        var checksByRuleId = existingChecks.ToDictionary(x => x.StrategyRuleId);
        foreach (var item in items)
        {
            var note = string.IsNullOrWhiteSpace(item.Note) ? null : item.Note.Trim();
            if (checksByRuleId.TryGetValue(item.StrategyRuleId, out var check))
            {
                check.IsFollowed = item.IsFollowed;
                check.Note = note;
                check.ModifiedByUserId = userId;
                await repository.UpdateAsync(check, cancellationToken).ConfigureAwait(false);
                continue;
            }

            await repository.AddAsync(new TradeRuleCheck
            {
                TradeId = trade.Id,
                StrategyRuleId = item.StrategyRuleId,
                UserId = userId,
                IsFollowed = item.IsFollowed,
                Note = note,
                IsActive = true,
                CreatedByUserId = userId,
                ModifiedByUserId = userId
            }, cancellationToken).ConfigureAwait(false);
        }

        var model = await TradeRuleCheckResponseFactory
            .CreateAsync(repository, trade, userId, cancellationToken)
            .ConfigureAwait(false);

        return new StandardResponse<GetTradeRuleChecksResponse>(OperationResult.Updated, model);
    }

    private sealed class RuleIdRow
    {
        public Guid Id { get; set; }
    }
}
