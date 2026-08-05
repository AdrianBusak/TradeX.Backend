using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.TradingAccounts.Commands;

public sealed class HardDeleteTradingAccountCommand(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<HardDeleteEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class HardDeleteTradingAccountCommandValidator
        : AbstractValidator<HardDeleteTradingAccountCommand>
    {
        public HardDeleteTradingAccountCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}

public sealed class HardDeleteTradingAccountCommandHandler(ITradeXRepository repository)
    : IRequestHandler<HardDeleteTradingAccountCommand, StandardResponse<HardDeleteEntityResponseModel>>
{
    public async Task<StandardResponse<HardDeleteEntityResponseModel>> Handle(
        HardDeleteTradingAccountCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        var entity = await repository.GetSingleAsync<TradingAccount>(
                account => account.Id == request.Id && account.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<HardDeleteEntityResponseModel>(
                request.Id,
                nameof(TradingAccount));
        }

        if (await HasTradeAssignmentsAsync(request.Id, cancellationToken).ConfigureAwait(false))
        {
            return new StandardResponse<HardDeleteEntityResponseModel>(
                OperationResult.BadRequest,
                "Entity has related records.",
                null!);
        }

        await repository.DeleteHardAsync<TradingAccount>(request.Id, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<HardDeleteEntityResponseModel>(
            OperationResult.Deleted,
            new HardDeleteEntityResponseModel());
    }

    private async Task<bool> HasTradeAssignmentsAsync(
        Guid tradingAccountId,
        CancellationToken cancellationToken)
    {
        var query =
            from assignment in repository.DbContext.TradeAccountAssignment
            where assignment.TradingAccountId == tradingAccountId
            select new RelatedEntityResponseModel
            {
                Id = assignment.Id
            };

        var result = await repository.QueryAsync(
                query,
                pageSize: 1,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result.Records is { Count: > 0 };
    }

    private sealed class RelatedEntityResponseModel
    {
        public Guid Id { get; set; }
    }
}
