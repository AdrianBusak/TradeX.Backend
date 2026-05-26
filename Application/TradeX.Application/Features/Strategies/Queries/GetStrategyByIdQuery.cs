using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.Strategies.Queries;

public sealed class GetStrategyByIdQuery(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<GetStrategyByIdResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class GetStrategyByIdQueryValidator : AbstractValidator<GetStrategyByIdQuery>
    {
        public GetStrategyByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}

public sealed class GetStrategyByIdQueryHandler(ITradeXRepository repository)
    : IRequestHandler<GetStrategyByIdQuery, StandardResponse<GetStrategyByIdResponseModel>>
{
    public async Task<StandardResponse<GetStrategyByIdResponseModel>> Handle(
        GetStrategyByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        var entity = await repository.GetSingleAsync<Strategy>(
                strategy => strategy.Id == request.Id && strategy.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<GetStrategyByIdResponseModel>(
                request.Id,
                nameof(Strategy));
        }

        return new StandardResponse<GetStrategyByIdResponseModel>(
            OperationResult.Ok,
            new GetStrategyByIdResponseModel
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                MarketType = entity.MarketType,
                Color = entity.Color,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            });
    }
}

public sealed class GetStrategyByIdResponseModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public MarketType MarketType { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}
