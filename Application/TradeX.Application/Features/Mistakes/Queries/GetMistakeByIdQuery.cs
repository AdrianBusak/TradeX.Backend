using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Clients.Features.Mistakes;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Mistakes.Queries;

public sealed class GetMistakeByIdQuery(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<MistakeResponse>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class Validator : AbstractValidator<GetMistakeByIdQuery>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}

public sealed class GetMistakeByIdQueryHandler(ITradeXRepository repository)
    : IRequestHandler<GetMistakeByIdQuery, StandardResponse<MistakeResponse>>
{
    public async Task<StandardResponse<MistakeResponse>> Handle(
        GetMistakeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var mistake = await repository.GetSingleAsync<Mistake>(
                entity => entity.Id == request.Id && entity.UserId == request.UserId(),
                cancellationToken)
            .ConfigureAwait(false);

        if (mistake is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<MistakeResponse>(
                request.Id,
                nameof(Mistake));
        }

        var response = new MistakeResponse
        {
            Id = mistake.Id,
            Name = mistake.Name,
            Description = mistake.Description,
            IsActive = mistake.IsActive
        };

        return new StandardResponse<MistakeResponse>(OperationResult.Ok, response);
    }
}
