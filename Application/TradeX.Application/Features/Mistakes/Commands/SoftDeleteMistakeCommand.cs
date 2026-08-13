using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Mistakes.Commands;

public sealed class SoftDeleteMistakeCommand(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<SoftDeleteEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class Validator : AbstractValidator<SoftDeleteMistakeCommand>
    {
        public Validator() => RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class SoftDeleteMistakeCommandHandler(ITradeXRepository repository)
    : IRequestHandler<SoftDeleteMistakeCommand, StandardResponse<SoftDeleteEntityResponseModel>>
{
    public async Task<StandardResponse<SoftDeleteEntityResponseModel>> Handle(
        SoftDeleteMistakeCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await repository.GetSingleAsync<Mistake>(
                mistake => mistake.Id == request.Id && mistake.UserId == request.UserId(),
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<SoftDeleteEntityResponseModel>(
                request.Id,
                nameof(Mistake));
        }

        if (!entity.IsActive)
        {
            return new StandardResponse<SoftDeleteEntityResponseModel>(
                OperationResult.BadRequest,
                "Mistake is already deleted.",
                null!);
        }

        entity.IsActive = false;
        entity.ModifiedByUserId = request.UserId();
        await repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<SoftDeleteEntityResponseModel>(
            OperationResult.Deleted,
            new SoftDeleteEntityResponseModel());
    }
}