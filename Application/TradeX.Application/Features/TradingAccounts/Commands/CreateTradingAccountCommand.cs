using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;
using static TradeX.Application.Clients.Features.TradingAccounts.Commands.CreateTradingAccountCommand;

namespace TradeX.Application.Clients.Features.TradingAccounts.Commands;

public sealed class CreateTradingAccountCommand(CreateTradingAccountCommandModel data)
    : BaseInput<CreateTradingAccountCommandModel>(data),
      IRequest<StandardResponse<CreateEntityResponseModel>>,
      IAuthenticatedRequest
{
    public sealed class CreateTradingAccountCommandModel
    {
        public string Name { get; set; } = null!;
        public TradingAccountType AccountType { get; set; }
        public string Broker { get; set; } = null!;
        public string Currency { get; set; } = null!;
        public decimal InitialBalance { get; set; }
        public decimal? CurrentBalance { get; set; }
    }

    public sealed class CreateTradingAccountCommandValidator : AbstractValidator<CreateTradingAccountCommand>
    {
        public CreateTradingAccountCommandValidator()
        {
            RuleFor(x => x.Model)
                .NotEmpty()
                .SetValidator(new CreateTradingAccountCommandModelValidator());
        }
    }

    public sealed class CreateTradingAccountCommandModelValidator : AbstractValidator<CreateTradingAccountCommandModel>
    {
        public CreateTradingAccountCommandModelValidator()
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
                .GreaterThanOrEqualTo(0)
                .When(x => x.CurrentBalance.HasValue);
        }
    }
}

public sealed class CreateTradingAccountCommandHandler(ITradeXRepository repository)
    : IRequestHandler<CreateTradingAccountCommand, StandardResponse<CreateEntityResponseModel>>
{
    public async Task<StandardResponse<CreateEntityResponseModel>> Handle(
        CreateTradingAccountCommand request,
        CancellationToken cancellationToken)
    {
        var name = NormalizeRequired(request.Model.Name);
        var userId = request.UserId();

        if (await NameExistsAsync(userId, name, cancellationToken).ConfigureAwait(false))
        {
            return new StandardResponse<CreateEntityResponseModel>(
                OperationResult.Conflict,
                "Entity with the same name already exists.",
                null!);
        }

        var entity = new TradingAccount
        {
            UserId = userId,
            Name = name,
            AccountType = request.Model.AccountType,
            Broker = NormalizeRequired(request.Model.Broker),
            Currency = NormalizeCurrency(request.Model.Currency),
            InitialBalance = request.Model.InitialBalance,
            CurrentBalance = request.Model.CurrentBalance ?? request.Model.InitialBalance,
            IsActive = true,
            CreatedByUserId = userId,
            ModifiedByUserId = userId
        };

        var id = await repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);

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

        var existingId = await repository.GetIdAsync<TradingAccount>(
                account => account.UserId == userId &&
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
