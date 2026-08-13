using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Mistakes.Commands;

public sealed class RestoreMistakeCommand(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<RestoreEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class Validator : AbstractValidator<RestoreMistakeCommand>
    {
        public Validator() => RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class RestoreMistakeCommandHandler(ITradeXRepository repository)
    : IRequestHandler<RestoreMistakeCommand, StandardResponse<RestoreEntityResponseModel>>
{
    public async Task<StandardResponse<RestoreEntityResponseModel>> Handle(
        RestoreMistakeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetSingleAsync<Mistake>(
                mistake => mistake.Id == request.Id && mistake.UserId == request.UserId(),
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<RestoreEntityResponseModel>(
                request.Id,
                nameof(Mistake));
        }

        if (entity.IsActive)
        {
            return new StandardResponse<RestoreEntityResponseModel>(
                OperationResult.BadRequest,
                "Mistake is already active.",
                null!);
        }

        entity.IsActive = true;
        entity.ModifiedByUserId = request.UserId();
        await repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<RestoreEntityResponseModel>(
            OperationResult.Updated,
            new RestoreEntityResponseModel());
    }
}
