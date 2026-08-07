using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Clients.Features.LotCalculator.Services;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Features.LotCalculator.Commands;

public sealed class CalculateLotRequest
{
    public Guid? TradingAccountId { get; set; }
    public string? AccountCurrency { get; set; }
    public decimal? AccountBalance { get; set; }
    public decimal RiskPercent { get; set; }
    public Guid? TradingInstrumentId { get; set; }
    public string? Symbol { get; set; }
    public MarketType? MarketType { get; set; }
    public decimal? EntryPrice { get; set; }
    public decimal? StopLossPrice { get; set; }
    public decimal? StopLossPips { get; set; }
}

public sealed class CalculateLotResponse
{
    public string Symbol { get; set; } = null!;
    public MarketType MarketType { get; set; }
    public string AccountCurrency { get; set; } = null!;
    public decimal AccountBalance { get; set; }
    public decimal RiskPercent { get; set; }
    public decimal RiskAmount { get; set; }
    public decimal StopLossPips { get; set; }
    public decimal PipSize { get; set; }
    public decimal ContractSize { get; set; }
    public decimal PipValuePerLot { get; set; }
    public decimal LotSize { get; set; }
    public decimal RoundedLotSize { get; set; }
    public decimal EstimatedLoss { get; set; }
    public string? Warning { get; set; }
}

public sealed class CalculateLotCommand(CalculateLotRequest data)
    : BaseInput<CalculateLotRequest>(data),
      IRequest<StandardResponse<CalculateLotResponse>>,
      IAuthenticatedRequest
{
    public sealed class CalculateLotCommandValidator : AbstractValidator<CalculateLotCommand>
    {
        public CalculateLotCommandValidator()
        {
            RuleFor(x => x.Model).NotNull().SetValidator(new CalculateLotRequestValidator());
        }
    }

    public sealed class CalculateLotRequestValidator : AbstractValidator<CalculateLotRequest>
    {
        public CalculateLotRequestValidator()
        {
            RuleFor(x => x.RiskPercent).GreaterThan(0).LessThanOrEqualTo(100);
            RuleFor(x => x.AccountBalance).GreaterThan(0).When(x => !x.TradingAccountId.HasValue);
            RuleFor(x => x.AccountCurrency).NotEmpty().Length(3).When(x => !x.TradingAccountId.HasValue);
            RuleFor(x => x.Symbol).NotEmpty().When(x => !x.TradingInstrumentId.HasValue);
            RuleFor(x => x.MarketType).NotNull().IsInEnum().When(x => !x.TradingInstrumentId.HasValue);
            RuleFor(x => x.StopLossPips).GreaterThan(0).When(x => x.StopLossPips.HasValue);
            RuleFor(x => x).Must(HasStopLoss).WithMessage("Provide StopLossPips, or distinct positive EntryPrice and StopLossPrice values.");
        }

        private static bool HasStopLoss(CalculateLotRequest request) =>
            request.StopLossPips is > 0 ||
            (request.EntryPrice is > 0 && request.StopLossPrice is > 0 && request.EntryPrice != request.StopLossPrice);
    }
}

public sealed class CalculateLotCommandHandler(
    ITradeXRepository repository,
    ILotCalculatorService lotCalculatorService)
    : IRequestHandler<CalculateLotCommand, StandardResponse<CalculateLotResponse>>
{
    public async Task<StandardResponse<CalculateLotResponse>> Handle(
        CalculateLotCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();
        var account = await ResolveAccountAsync(request.Model, userId, cancellationToken).ConfigureAwait(false);
        if (account.Error is not null)
        {
            return new StandardResponse<CalculateLotResponse>(OperationResult.NotFound, account.Error, null!);
        }

        var instrument = await ResolveInstrumentAsync(request.Model, userId, cancellationToken).ConfigureAwait(false);
        if (instrument.Error is not null)
        {
            return new StandardResponse<CalculateLotResponse>(OperationResult.NotFound, instrument.Error, null!);
        }

        var accountCurrency = account.Currency!;
        var accountBalance = account.Balance;
        if (accountBalance <= 0)
        {
            return new StandardResponse<CalculateLotResponse>(OperationResult.BadRequest, "Account balance must be greater than zero.", null!);
        }

        try
        {
            var result = await lotCalculatorService.CalculateAsync(new LotCalculationInput
            {
                Symbol = instrument.Symbol!,
                MarketType = instrument.MarketType!.Value,
                AccountCurrency = accountCurrency,
                AccountBalance = accountBalance,
                RiskPercent = request.Model.RiskPercent,
                EntryPrice = request.Model.EntryPrice,
                StopLossPrice = request.Model.StopLossPrice,
                StopLossPips = request.Model.StopLossPips
            }, cancellationToken).ConfigureAwait(false);

            return new StandardResponse<CalculateLotResponse>(OperationResult.Ok, new CalculateLotResponse
            {
                Symbol = result.Symbol,
                MarketType = result.MarketType,
                AccountCurrency = accountCurrency.Trim().ToUpperInvariant(),
                AccountBalance = accountBalance,
                RiskPercent = request.Model.RiskPercent,
                RiskAmount = accountBalance * request.Model.RiskPercent / 100m,
                StopLossPips = result.StopLossPips,
                PipSize = result.PipSize,
                ContractSize = result.ContractSize,
                PipValuePerLot = result.PipValuePerLot,
                LotSize = result.LotSize,
                RoundedLotSize = result.RoundedLotSize,
                EstimatedLoss = result.EstimatedLoss,
                Warning = result.Warning
            });
        }
        catch (ArgumentException exception)
        {
            return new StandardResponse<CalculateLotResponse>(OperationResult.BadRequest, exception.Message, null!);
        }
        catch (InvalidOperationException exception)
        {
            return new StandardResponse<CalculateLotResponse>(OperationResult.BadRequest, exception.Message, null!);
        }
    }

    private async Task<(string? Currency, decimal Balance, string? Error)> ResolveAccountAsync(
        CalculateLotRequest model, Guid userId, CancellationToken cancellationToken)
    {
        if (!model.TradingAccountId.HasValue)
        {
            return (model.AccountCurrency, model.AccountBalance ?? 0, null);
        }

        var account = await repository.GetSingleAsync<TradingAccount>(
            x => x.Id == model.TradingAccountId.Value && x.UserId == userId,
            cancellationToken).ConfigureAwait(false);
        if (account is null)
        {
            return (null, 0, "Trading account was not found.");
        }

        return (account.Currency, account.CurrentBalance > 0 ? account.CurrentBalance : account.InitialBalance, null);
    }

    private async Task<(string? Symbol, MarketType? MarketType, string? Error)> ResolveInstrumentAsync(
        CalculateLotRequest model, Guid userId, CancellationToken cancellationToken)
    {
        if (!model.TradingInstrumentId.HasValue)
        {
            return (model.Symbol, model.MarketType, null);
        }

        var instrument = await repository.GetSingleAsync<TradingInstrument>(
            x => x.Id == model.TradingInstrumentId.Value && x.UserId == userId,
            cancellationToken).ConfigureAwait(false);
        return instrument is null
            ? (null, null, "Trading instrument was not found.")
            : (instrument.Symbol, instrument.MarketType, null);
    }
}
