using TradeX.Application.Clients.Features.Trades.Commands;
using TradeX.Domain.Enums;

namespace TradeX.Application.Clients.Tests.Features.Trades;

public class TradeRiskDistanceValidationTests
{
    [Fact]
    public async Task Validator_AcceptsPositiveStopLossAndTakeProfitIndependentOfEntryPriceAndDirection()
    {
        var model = CreateValidModel();
        model.Direction = TradeDirection.Long;
        model.EntryPrice = 100m;
        model.StopLoss = 50;
        model.TakeProfit = 100;

        var result = await new CreateTradeCommand.CreateTradeCommandModelValidator()
            .ValidateAsync(model);

        Assert.True(result.IsValid, string.Join(Environment.NewLine, result.Errors));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validator_RejectsNonPositiveStopLoss(int stopLoss)
    {
        var model = CreateValidModel();
        model.StopLoss = stopLoss;

        var result = await new CreateTradeCommand.CreateTradeCommandModelValidator()
            .ValidateAsync(model);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(model.StopLoss));
    }

    private static CreateTradeCommand.CreateTradeCommandModel CreateValidModel() => new()
    {
        StrategyId = Guid.NewGuid(),
        TradingInstrumentId = Guid.NewGuid(),
        TradingAccountIds = [Guid.NewGuid()],
        Direction = TradeDirection.Short,
        Status = TradeStatus.Planned,
        TradeDate = DateTime.UtcNow,
        StopLoss = 25,
        TakeProfit = 50
    };
}
