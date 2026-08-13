using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Clients.Features.Trades;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Trades.Commands;

public sealed class UpdateTradeMistakesRequest
{
    public List<UpdateTradeMistakeItemRequest> Mistakes { get; set; } = [];
}

public sealed class UpdateTradeMistakeItemRequest
{
    public Guid MistakeId { get; set; }
    public string? Note { get; set; }
}

public sealed class UpdateTradeMistakesCommand(Guid tradeId, UpdateTradeMistakesRequest data)
    : BaseInput<UpdateTradeMistakesRequest>(data),
      IRequest<StandardResponse<GetTradeMistakesResponse>>,
      IAuthenticatedRequest
{
    public Guid TradeId { get; } = tradeId;

    public sealed class Validator : AbstractValidator<UpdateTradeMistakesCommand>
    {
        public Validator()
        {
            RuleFor(x => x.TradeId).NotEmpty();
            RuleFor(x => x.Model).NotNull().SetValidator(new RequestValidator());
        }
    }

    public sealed class RequestValidator : AbstractValidator<UpdateTradeMistakesRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Mistakes).NotNull();
            RuleFor(x => x.Mistakes)
                .Must(mistakes =>
                    mistakes is null ||
                    mistakes.Select(x => x.MistakeId).Distinct().Count() == mistakes.Count)
                .WithMessage("Each mistake can be attached only once.");
            RuleForEach(x => x.Mistakes).ChildRules(item =>
            {
                item.RuleFor(x => x.MistakeId).NotEmpty();
                item.RuleFor(x => x.Note).MaximumLength(1000);
            });
        }
    }
}

public sealed class UpdateTradeMistakesCommandHandler(ITradeXRepository repository)
    : IRequestHandler<UpdateTradeMistakesCommand, StandardResponse<GetTradeMistakesResponse>>
{
    public async Task<StandardResponse<GetTradeMistakesResponse>> Handle(
        UpdateTradeMistakesCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var trade = await repository.GetSingleAsync<Trade>(
                entity => entity.Id == request.TradeId && entity.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (trade is null)
        {
            return new StandardResponse<GetTradeMistakesResponse>(
                OperationResult.NotFound,
                "Trade was not found.",
                null!);
        }

        var items = request.Model.Mistakes;
        var mistakeIds = items.Select(x => x.MistakeId).ToList();
        var validMistakes = mistakeIds.Count == 0
            ? []
            : await repository.GetListAsync<Mistake>(
                    cancellationToken,
                    mistake => mistake.UserId == userId &&
                               mistake.IsActive &&
                               mistakeIds.Contains(mistake.Id))
                .ConfigureAwait(false);

        if (validMistakes.Count != mistakeIds.Count)
        {
            return new StandardResponse<GetTradeMistakesResponse>(
                OperationResult.NotFound,
                "One or more mistakes were not found in your active catalog.",
                null!);
        }

        await repository.DeleteHardWhereAsync<TradeMistake>(
                cancellationToken,
                mistake => mistake.TradeId == trade.Id)
            .ConfigureAwait(false);

        if (items.Count > 0)
        {
            var tradeMistakes = items.Select(item => new TradeMistake
            {
                TradeId = trade.Id,
                MistakeId = item.MistakeId,
                Note = string.IsNullOrWhiteSpace(item.Note) ? null : item.Note.Trim(),
                IsActive = true,
                CreatedByUserId = userId,
                ModifiedByUserId = userId
            }).ToList();

            await repository.AddRangeAsync(tradeMistakes, cancellationToken).ConfigureAwait(false);
        }

        var model = await TradeMistakeResponseFactory
            .CreateAsync(repository, trade, cancellationToken)
            .ConfigureAwait(false);

        return new StandardResponse<GetTradeMistakesResponse>(OperationResult.Updated, model);
    }
}
