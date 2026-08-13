using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using static TradeX.Application.Clients.Features.Mistakes.Commands.UpdateMistakeCommand;

namespace TradeX.Application.Clients.Features.Mistakes.Commands;

public sealed class UpdateMistakeCommand(Guid id, UpdateMistakeRequest data)
    : BaseInput<UpdateMistakeRequest>(data),
      IRequest<StandardResponse<UpdateEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class UpdateMistakeRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    public sealed class Validator : AbstractValidator<UpdateMistakeCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Model).NotNull().SetValidator(new RequestValidator());
        }
    }

    public sealed class RequestValidator : AbstractValidator<UpdateMistakeRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }
}

public sealed class UpdateMistakeCommandHandler(ITradeXRepository repository)
    : IRequestHandler<UpdateMistakeCommand, StandardResponse<UpdateEntityResponseModel>>
{
    public async Task<StandardResponse<UpdateEntityResponseModel>> Handle(
        UpdateMistakeCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var entity = await repository.GetSingleAsync<Mistake>(
                mistake => mistake.Id == request.Id && mistake.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<UpdateEntityResponseModel>(
                request.Id,
                nameof(Mistake));
        }

        var name = request.Model.Name.Trim();
        if (await MistakeNameValidator.NameExistsAsync(repository, userId, name, entity.Id, cancellationToken)
                .ConfigureAwait(false))
        {
            return new StandardResponse<UpdateEntityResponseModel>(
                OperationResult.Conflict,
                "A mistake with the same name already exists.",
                null!);
        }

        entity.Name = name;
        entity.Description = MistakeText.Normalize(request.Model.Description);
        entity.ModifiedByUserId = userId;

        await repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<UpdateEntityResponseModel>(
            OperationResult.Updated,
            new UpdateEntityResponseModel());
    }
}
