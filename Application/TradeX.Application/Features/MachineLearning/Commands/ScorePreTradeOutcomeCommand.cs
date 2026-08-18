using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.MachineLearning.Commands;

public sealed class ScorePreTradeOutcomeCommand(PreTradeScoreRequest data)
    : BaseInput<PreTradeScoreRequest>(data),
      IRequest<StandardResponse<PreTradeScoreResponse>>,
      IAuthenticatedRequest
{
    public sealed class Validator : AbstractValidator<ScorePreTradeOutcomeCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .SetValidator(new RequestValidator());
        }
    }

    public sealed class RequestValidator : AbstractValidator<PreTradeScoreRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.StrategyId).NotEmpty();
            RuleFor(x => x.TradingInstrumentId).NotEmpty();
            RuleFor(x => x.Direction).IsInEnum();
            RuleFor(x => x.TradeDate).NotEmpty();
            RuleFor(x => x.EntryPrice).GreaterThan(0).When(x => x.EntryPrice.HasValue);
            RuleFor(x => x.StopLoss).GreaterThan(0).When(x => x.StopLoss.HasValue);
            RuleFor(x => x.TakeProfit).GreaterThan(0).When(x => x.TakeProfit.HasValue);
            RuleFor(x => x.LotSize).GreaterThan(0).When(x => x.LotSize.HasValue);
            RuleFor(x => x.RiskAmount).GreaterThanOrEqualTo(0).When(x => x.RiskAmount.HasValue);
            RuleFor(x => x.RuleChecks)
                .Must(ruleChecks => ruleChecks.Select(x => x.StrategyRuleId).Distinct().Count() == ruleChecks.Count)
                .WithMessage("Each strategy rule can be checked only once.");
            RuleForEach(x => x.RuleChecks).ChildRules(rule =>
            {
                rule.RuleFor(x => x.StrategyRuleId).NotEmpty();
            });
        }
    }
}

public sealed class ScorePreTradeOutcomeCommandHandler(
    ITradeXRepository repository,
    ITradeOutcomeMlService service)
    : IRequestHandler<ScorePreTradeOutcomeCommand, StandardResponse<PreTradeScoreResponse>>
{
    public async Task<StandardResponse<PreTradeScoreResponse>> Handle(
        ScorePreTradeOutcomeCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var strategyExists = await repository.GetIdAsync<Strategy>(
                entity => entity.Id == request.Model.StrategyId && entity.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);
        var instrumentExists = await repository.GetIdAsync<TradingInstrument>(
                entity => entity.Id == request.Model.TradingInstrumentId &&
                          entity.UserId == userId &&
                          entity.IsActive,
                cancellationToken)
            .ConfigureAwait(false);

        if (!strategyExists.HasValue || !instrumentExists.HasValue)
        {
            return new StandardResponse<PreTradeScoreResponse>(
                OperationResult.NotFound,
                "Strategy or trading instrument was not found.",
                null!);
        }

        var ruleIds = request.Model.RuleChecks.Select(x => x.StrategyRuleId).ToList();
        if (ruleIds.Count > 0)
        {
            var validRuleIds = await repository.QueryAsync(
                    from rule in repository.DbContext.StrategyRule
                    where rule.StrategyId == request.Model.StrategyId &&
                          rule.IsActive &&
                          ruleIds.Contains(rule.Id)
                    select new RuleIdRow { Id = rule.Id },
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if ((validRuleIds.Records?.Count ?? 0) != ruleIds.Count)
            {
                return new StandardResponse<PreTradeScoreResponse>(
                    OperationResult.NotFound,
                    "One or more strategy rules were not found for this strategy.",
                    null!);
            }
        }

        var response = await service.ScoreAsync(userId, request.Model, cancellationToken)
            .ConfigureAwait(false);

        return new StandardResponse<PreTradeScoreResponse>(
            response.IsReady ? OperationResult.Ok : OperationResult.BadRequest,
            response);
    }

    private sealed class RuleIdRow
    {
        public Guid Id { get; set; }
    }
}
