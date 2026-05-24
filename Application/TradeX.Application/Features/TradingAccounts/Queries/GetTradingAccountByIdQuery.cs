using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.TradingAccounts.Queries;

public sealed class GetTradingAccountByIdQuery(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<GetTradingAccountByIdResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class GetTradingAccountByIdQueryValidator : AbstractValidator<GetTradingAccountByIdQuery>
    {
        public GetTradingAccountByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}

public sealed class GetTradingAccountByIdQueryHandler(ITradeXRepository repository)
    : IRequestHandler<GetTradingAccountByIdQuery, StandardResponse<GetTradingAccountByIdResponseModel>>
{
    public async Task<StandardResponse<GetTradingAccountByIdResponseModel>> Handle(
        GetTradingAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        var entity = await repository.GetSingleAsync<TradingAccount>(
                account => account.Id == request.Id && account.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<GetTradingAccountByIdResponseModel>(
                request.Id,
                nameof(TradingAccount));
        }

        return new StandardResponse<GetTradingAccountByIdResponseModel>(
            OperationResult.Ok,
            new GetTradingAccountByIdResponseModel
            {
                Id = entity.Id,
                Name = entity.Name,
                AccountType = entity.AccountType,
                Broker = entity.Broker,
                Currency = entity.Currency,
                InitialBalance = entity.InitialBalance,
                CurrentBalance = entity.CurrentBalance,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt,
                ModifiedAt = entity.ModifiedAt
            });
    }
}

public sealed class GetTradingAccountByIdResponseModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public TradingAccountType AccountType { get; set; }
    public string Broker { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public decimal InitialBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}
