using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TradeX.Application.Abstractions.Constants;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Services;
using TradeX.Application.Clients.Features.StrategyRules.Commands;
using TradeX.Application.Clients.Features.Trades.Commands;
using TradeX.Application.Clients.Features.Trades.Queries;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;
using TradeX.Repository;
using TradeX.Repository.Services;

namespace TradeX.Application.Clients.Tests.Features.Trades;

public class TradeRuleCheckHandlersTests
{
    [Fact]
    public async Task GetRuleChecks_ReturnsActiveRulesAndComplianceSummary()
    {
        var (db, repository) = CreateRepository();
        var userId = Guid.NewGuid();
        var strategy = CreateStrategy(userId);
        var trade = CreateTrade(userId, strategy.Id);
        var followedRule = CreateRule(strategy.Id, 1);
        var brokenRule = CreateRule(strategy.Id, 2);
        var uncheckedRule = CreateRule(strategy.Id, 3);
        db.AddRange(strategy, trade, followedRule, brokenRule, uncheckedRule,
            CreateCheck(trade.Id, followedRule.Id, userId, true),
            CreateCheck(trade.Id, brokenRule.Id, userId, false, "Entered early"));
        await db.SaveChangesAsync();

        var response = await new GetTradeRuleChecksQueryHandler(repository).Handle(
            WithUser(new GetTradeRuleChecksQuery(trade.Id), userId), CancellationToken.None);

        Assert.Equal(OperationResult.Ok, response.Result);
        Assert.Equal(3, response.Model!.TotalRules);
        Assert.Equal(2, response.Model.CheckedRules);
        Assert.Equal(1, response.Model.FollowedRules);
        Assert.Equal(1, response.Model.BrokenRules);
        Assert.Equal(50m, response.Model.ComplianceScore);
        Assert.Null(response.Model.Rules.Single(x => x.StrategyRuleId == uncheckedRule.Id).IsFollowed);
    }

    [Fact]
    public async Task UpdateRuleChecks_UpsertsAndPreservesOmittedChecks()
    {
        var (db, repository) = CreateRepository();
        var userId = Guid.NewGuid();
        var strategy = CreateStrategy(userId);
        var trade = CreateTrade(userId, strategy.Id);
        var firstRule = CreateRule(strategy.Id, 1);
        var secondRule = CreateRule(strategy.Id, 2);
        db.AddRange(strategy, trade, firstRule, secondRule);
        await db.SaveChangesAsync();

        var handler = new UpdateTradeRuleChecksCommandHandler(repository);
        var firstResponse = await handler.Handle(
            WithUser(new UpdateTradeRuleChecksCommand(trade.Id, new UpdateTradeRuleChecksRequest
            {
                Rules =
                [
                    new() { StrategyRuleId = firstRule.Id, IsFollowed = true },
                    new() { StrategyRuleId = secondRule.Id, IsFollowed = false, Note = "  Missed confirmation  " }
                ]
            }), userId), CancellationToken.None);

        db.ChangeTracker.Clear();

        var secondResponse = await handler.Handle(
            WithUser(new UpdateTradeRuleChecksCommand(trade.Id, new UpdateTradeRuleChecksRequest
            {
                Rules = [new() { StrategyRuleId = firstRule.Id, IsFollowed = false, Note = "Changed" }]
            }), userId), CancellationToken.None);

        Assert.Equal(OperationResult.Updated, firstResponse.Result);
        Assert.Equal(OperationResult.Updated, secondResponse.Result);
        Assert.Equal(2, await db.TradeRuleCheck.CountAsync());
        Assert.Equal(0m, secondResponse.Model!.ComplianceScore);
        Assert.Equal("Changed", (await db.TradeRuleCheck.SingleAsync(x => x.StrategyRuleId == firstRule.Id)).Note);
        Assert.Equal("Missed confirmation", (await db.TradeRuleCheck.SingleAsync(x => x.StrategyRuleId == secondRule.Id)).Note);
    }

    [Fact]
    public async Task UpdateRuleChecks_WhenRuleBelongsToAnotherStrategy_ReturnsNotFound()
    {
        var (db, repository) = CreateRepository();
        var userId = Guid.NewGuid();
        var tradeStrategy = CreateStrategy(userId);
        var otherStrategy = CreateStrategy(userId);
        var trade = CreateTrade(userId, tradeStrategy.Id);
        var otherRule = CreateRule(otherStrategy.Id, 1);
        db.AddRange(tradeStrategy, otherStrategy, trade, otherRule);
        await db.SaveChangesAsync();

        var response = await new UpdateTradeRuleChecksCommandHandler(repository).Handle(
            WithUser(new UpdateTradeRuleChecksCommand(trade.Id, new UpdateTradeRuleChecksRequest
            {
                Rules = [new() { StrategyRuleId = otherRule.Id, IsFollowed = true }]
            }), userId), CancellationToken.None);

        Assert.Equal(OperationResult.NotFound, response.Result);
        Assert.Empty(await db.TradeRuleCheck.ToListAsync());
    }

    [Fact]
    public async Task UpdateRuleChecks_WhenTradeBelongsToAnotherUser_ReturnsNotFound()
    {
        var (db, repository) = CreateRepository();
        var ownerId = Guid.NewGuid();
        var strategy = CreateStrategy(ownerId);
        var trade = CreateTrade(ownerId, strategy.Id);
        var rule = CreateRule(strategy.Id, 1);
        db.AddRange(strategy, trade, rule);
        await db.SaveChangesAsync();

        var response = await new UpdateTradeRuleChecksCommandHandler(repository).Handle(
            WithUser(new UpdateTradeRuleChecksCommand(trade.Id, new UpdateTradeRuleChecksRequest
            {
                Rules = [new() { StrategyRuleId = rule.Id, IsFollowed = true }]
            }), Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(OperationResult.NotFound, response.Result);
    }

    [Fact]
    public async Task UpdateRuleChecks_WhenTradeIsClosed_UpdatesChecklist()
    {
        var (db, repository) = CreateRepository();
        var userId = Guid.NewGuid();
        var strategy = CreateStrategy(userId);
        var trade = CreateTrade(userId, strategy.Id);
        trade.Status = TradeStatus.Closed;
        var rule = CreateRule(strategy.Id, 1);
        db.AddRange(strategy, trade, rule, CreateCheck(trade.Id, rule.Id, userId, true));
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var response = await new UpdateTradeRuleChecksCommandHandler(repository).Handle(
            WithUser(new UpdateTradeRuleChecksCommand(trade.Id, new UpdateTradeRuleChecksRequest
            {
                Rules = [new() { StrategyRuleId = rule.Id, IsFollowed = false }]
            }), userId), CancellationToken.None);

        Assert.Equal(OperationResult.Updated, response.Result);
        Assert.False((await db.TradeRuleCheck.SingleAsync()).IsFollowed);
    }

    [Fact]
    public async Task UpdateRuleChecksValidator_RejectsDuplicateRulesAndLongNotes()
    {
        var ruleId = Guid.NewGuid();
        var result = await new UpdateTradeRuleChecksCommand.Validator().ValidateAsync(
            new UpdateTradeRuleChecksCommand(Guid.NewGuid(), new UpdateTradeRuleChecksRequest
            {
                Rules =
                [
                    new() { StrategyRuleId = ruleId, IsFollowed = true },
                    new() { StrategyRuleId = ruleId, IsFollowed = false, Note = new string('a', 1001) }
                ]
            }));

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task HardDeleteStrategyRule_WhenHistoricalChecksExist_ReturnsBadRequest()
    {
        var (db, repository) = CreateRepository();
        var userId = Guid.NewGuid();
        var strategy = CreateStrategy(userId);
        var trade = CreateTrade(userId, strategy.Id);
        var rule = CreateRule(strategy.Id, 1, isActive: false);
        db.AddRange(strategy, trade, rule, CreateCheck(trade.Id, rule.Id, userId, true));
        await db.SaveChangesAsync();

        var response = await new HardDeleteStrategyRuleCommandHandler(repository).Handle(
            WithUser(new HardDeleteStrategyRuleCommand(strategy.Id, rule.Id), userId), CancellationToken.None);

        Assert.Equal(OperationResult.BadRequest, response.Result);
        Assert.Equal(1, await db.StrategyRule.CountAsync());
    }

    private static TRequest WithUser<TRequest>(TRequest request, Guid userId)
        where TRequest : IContextualRequest
    {
        request.Context.Add(ContextKeys.UserId, userId);
        request.Context.Add(ContextKeys.ExternalUserId, $"idp|{userId}");
        return request;
    }

    private static Strategy CreateStrategy(Guid userId) => new()
    {
        Id = Guid.NewGuid(), UserId = userId, Name = "Strategy", MarketType = MarketType.Forex, IsActive = true
    };

    private static Trade CreateTrade(Guid userId, Guid strategyId) => new()
    {
        Id = Guid.NewGuid(), UserId = userId, StrategyId = strategyId,
        TradingInstrumentId = Guid.NewGuid(), TradeDate = DateTime.UtcNow, IsActive = true
    };

    private static StrategyRule CreateRule(Guid strategyId, int order, bool isActive = true) => new()
    {
        Id = Guid.NewGuid(), StrategyId = strategyId, Title = $"Rule {order}", Order = order,
        IsRequired = true, IsActive = isActive
    };

    private static TradeRuleCheck CreateCheck(Guid tradeId, Guid ruleId, Guid userId, bool followed, string? note = null) => new()
    {
        Id = Guid.NewGuid(), TradeId = tradeId, StrategyRuleId = ruleId, UserId = userId,
        IsFollowed = followed, Note = note, IsActive = true
    };

    private static (TradeXDbContext DbContext, TradeXRepository Repository) CreateRepository()
    {
        var options = new DbContextOptionsBuilder<TradeXDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TradeXDbContext(options);
        return (dbContext, new TradeXRepository(
            dbContext,
            NullLogger<RepositoryService<TradeXDbContext>>.Instance));
    }
}
