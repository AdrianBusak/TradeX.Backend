using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using static TradeX.Application.Clients.Features.Mistakes.Commands.CreateMistakeCommand;

namespace TradeX.Application.Clients.Features.Mistakes.Commands;

public sealed class CreateMistakeCommand(CreateMistakeRequest data)
    : BaseInput<CreateMistakeRequest>(data),
      IRequest<StandardResponse<CreateEntityResponseModel>>,
      IAuthenticatedRequest
{
    public sealed class CreateMistakeRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    public sealed class Validator : AbstractValidator<CreateMistakeCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Model)
                .NotNull()
                .SetValidator(new RequestValidator());
        }
    }

    public sealed class RequestValidator : AbstractValidator<CreateMistakeRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).MaximumLength(1000);
        }
    }
}

public sealed class CreateMistakeCommandHandler(ITradeXRepository repository)
    : IRequestHandler<CreateMistakeCommand, StandardResponse<CreateEntityResponseModel>>
{
    public async Task<StandardResponse<CreateEntityResponseModel>> Handle(
        CreateMistakeCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var name = NormalizeRequired(request.Model.Name);

        if (await NameExistsAsync(userId, name, cancellationToken)
                .ConfigureAwait(false))
        {
            return new StandardResponse<CreateEntityResponseModel>(
                OperationResult.Conflict,
                "A mistake with the same name already exists.",
                null!);
        }

        var id = await repository.AddAsync(new Mistake
        {
            UserId = userId,
            Name = name,
            Description = NormalizeOptional(request.Model.Description),
            IsActive = true,
            CreatedByUserId = userId,
            ModifiedByUserId = userId
        }, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<CreateEntityResponseModel>(
            OperationResult.Created,
            new CreateEntityResponseModel { Id = id });
    }

    private async Task<bool> NameExistsAsync(
        Guid userId,
        string name,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.ToUpperInvariant();

        return await repository.GetIdAsync<Mistake>(
                mistake => mistake.UserId == userId &&
                           mistake.Name.ToUpper() == normalizedName,
                cancellationToken)
            .ConfigureAwait(false) is not null;
    }

    private static string NormalizeRequired(string value)
        => value.Trim();

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
