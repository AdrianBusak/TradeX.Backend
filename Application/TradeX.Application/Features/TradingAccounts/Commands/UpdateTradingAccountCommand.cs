using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;
using static TradeX.Application.Clients.Features.TradingAccounts.Commands.UpdateTradingAccountCommand;

namespace TradeX.Application.Clients.Features.TradingAccounts.Commands;

public sealed class UpdateTradingAccountCommand(Guid id, UpdateTradingAccountCommandModel data)
    : BaseInput<UpdateTradingAccountCommandModel>(data),
      IRequest<StandardResponse<UpdateEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class UpdateTradingAccountCommandModel
    {
        public string Name { get; set; } = null!;
        public TradingAccountType AccountType { get; set; }
        public string Broker { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public decimal InitialBalance { get; set; }
        public decimal CurrentBalance { get; set; }
    }

    public sealed class UpdateTradingAccountCommandValidator : AbstractValidator<UpdateTradingAccountCommand>
    {
        public UpdateTradingAccountCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Model)
                .NotEmpty()
                .SetValidator(new UpdateTradingAccountCommandModelValidator());
        }
    }

    public sealed class UpdateTradingAccountCommandModelValidator : AbstractValidator<UpdateTradingAccountCommandModel>
    {
        public UpdateTradingAccountCommandModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.AccountType)
                .IsInEnum();

            RuleFor(x => x.Broker)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Currency)
                .NotEmpty()
                .Length(3);

            RuleFor(x => x.InitialBalance)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.CurrentBalance)
                .GreaterThanOrEqualTo(0);
        }
    }
}

public sealed class UpdateTradingAccountCommandHandler(ITradeXRepository repository)
    : IRequestHandler<UpdateTradingAccountCommand, StandardResponse<UpdateEntityResponseModel>>
{
    public async Task<StandardResponse<UpdateEntityResponseModel>> Handle(
        UpdateTradingAccountCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        var entity = await repository.GetSingleAsync<TradingAccount>(
                account => account.Id == request.Id && account.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (entity is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<UpdateEntityResponseModel>(
                request.Id,
                nameof(TradingAccount));
        }

        var name = NormalizeRequired(request.Model.Name);

        if (await NameExistsAsync(userId, entity.Id, name, cancellationToken).ConfigureAwait(false))
        {
            return new StandardResponse<UpdateEntityResponseModel>(
                OperationResult.Conflict,
                "Entity with the same name already exists.",
                null!);
        }

        entity.Name = name;
        entity.AccountType = request.Model.AccountType;
        entity.Broker = NormalizeRequired(request.Model.Broker);
        entity.Currency = NormalizeCurrency(request.Model.Currency);
        entity.InitialBalance = request.Model.InitialBalance;
        entity.CurrentBalance = request.Model.CurrentBalance;
        entity.ModifiedByUserId = userId;

        await repository.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);

        return new StandardResponse<UpdateEntityResponseModel>(
            OperationResult.Updated,
            new UpdateEntityResponseModel());
    }

    private async Task<bool> NameExistsAsync(
        Guid userId,
        Guid currentAccountId,
        string name,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.ToUpperInvariant();

        var existingId = await repository.GetIdAsync<TradingAccount>(
                account => account.UserId == userId &&
                           account.Id != currentAccountId &&
                           account.Name.ToUpper() == normalizedName,
                cancellationToken)
            .ConfigureAwait(false);

        return existingId.HasValue;
    }

    private static string NormalizeRequired(string value)
        => value.Trim();

    private static string NormalizeCurrency(string value)
        => value.Trim().ToUpperInvariant();
}
