using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Mistakes.Commands;

public sealed class HardDeleteMistakeCommand(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<HardDeleteEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class Validator : AbstractValidator<HardDeleteMistakeCommand>
    {
        public Validator() => RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class HardDeleteMistakeCommandHandler(ITradeXRepository repository)
    : IRequestHandler<HardDeleteMistakeCommand, StandardResponse<HardDeleteEntityResponseModel>>
{
    public async Task<StandardResponse<HardDeleteEntityResponseModel>> Handle(
        HardDeleteMistakeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetSingleAsync<Mistake>(
                mistake => mistake.Id == request.Id && mistake.UserId == request.UserId(),
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<HardDeleteEntityResponseModel>(
                request.Id,
                nameof(Mistake));
        }

        if (await repository.GetIdAsync<TradeMistake>(
                tradeMistake => tradeMistake.MistakeId == entity.Id,
                cancellationToken).ConfigureAwait(false) is not null)
        {
            return new StandardResponse<HardDeleteEntityResponseModel>(
                OperationResult.BadRequest,
                "Mistake cannot be hard deleted because it has trade history.",
                null!);
        }

        await repository.DeleteHardAsync<Mistake>(entity.Id, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<HardDeleteEntityResponseModel>(
            OperationResult.Deleted,
            new HardDeleteEntityResponseModel());
    }
}
