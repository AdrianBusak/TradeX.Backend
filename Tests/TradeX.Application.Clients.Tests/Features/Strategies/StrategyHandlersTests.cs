using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TradeX.Application.Abstractions.Configuration;
using TradeX.Application.Abstractions.Constants;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Services;
using TradeX.Application.Clients.Features.Strategies.Commands;
using TradeX.Application.Clients.Features.Strategies.Queries;
using TradeX.Application.Clients.Features.StrategyRules.Commands;
using TradeX.Domain.Entities;
using TradeX.Domain.Enums;
using TradeX.Repository;
using TradeX.Repository.Services;

namespace TradeX.Application.Clients.Tests.Features.Strategies;

public class StrategyHandlersTests
{
    [Fact]
    public async Task CreateStrategy_SetsCurrentUserAndNormalizesFields()
    {
        var (dbContext, repository) = CreateRepository();
        var userId = Guid.NewGuid();
        var handler = new CreateStrategyCommandHandler(repository);
        var request = WithUser(
            new CreateStrategyCommand(
                new CreateStrategyCommand.CreateStrategyCommandModel
                {
                    Name = " Breakout ",
                    Description = " London session ",
                    MarketType = MarketType.Forex,
                    Color = " #0A1b2C "
                }),
            userId);

        var response = await handler.Handle(request, CancellationToken.None);

        var strategy = await dbContext.Strategy.SingleAsync();
        Assert.Equal(OperationResult.Created, response.Result);
        Assert.Equal(userId, strategy.UserId);
        Assert.Equal("Breakout", strategy.Name);
        Assert.Equal("London session", strategy.Description);
        Assert.Equal(MarketType.Forex, strategy.MarketType);
        Assert.Equal("#0A1b2C", strategy.Color);
        Assert.True(strategy.IsActive);
        Assert.Equal(userId, strategy.CreatedByUserId);
        Assert.Equal(userId, strategy.ModifiedByUserId);
    }

    [Fact]
    public async Task CreateStrategy_WhenNameExistsForUser_ReturnsConflict()
    {
        var (dbContext, repository) = CreateRepository();
        var userId = Guid.NewGuid();
        dbContext.Strategy.Add(CreateStrategy(userId, name: "Breakout"));
        await dbContext.SaveChangesAsync();

        var handler = new CreateStrategyCommandHandler(repository);
        var request = WithUser(
            new CreateStrategyCommand(
                new CreateStrategyCommand.CreateStrategyCommandModel
                {
                    Name = " breakout ",
                    MarketType = MarketType.Forex
                }),
            userId);

        var response = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(OperationResult.Conflict, response.Result);
        Assert.Equal(1, await dbContext.Strategy.CountAsync());
    }

    [Fact]
    public async Task GetStrategies_ReturnsOnlyCurrentUsersStrategiesIncludingInactive()
    {
        var (dbContext, repository) = CreateRepository();
        var userId = Guid.NewGuid();
        dbContext.Strategy.AddRange(
            CreateStrategy(userId, name: "Active", isActive: true),
            CreateStrategy(userId, name: "Inactive", isActive: false),
            CreateStrategy(Guid.NewGuid(), name: "Other User", isActive: true));
        await dbContext.SaveChangesAsync();

        var handler = new GetStrategiesQueryHandler(
            repository,
            new ApplicationConfiguration
            {
                DataRetrievalConfiguration = new DataRetrievalConfiguration { DefaultPageSize = 10 }
            });
        var request = WithUser(new GetStrategiesQuery(), userId);

        var response = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(OperationResult.Ok, response.Result);
        Assert.Equal(2, response.TotalRecordCount);
        Assert.Contains(response.Model!, strategy => !strategy.IsActive);
        Assert.DoesNotContain(response.Model!, strategy => strategy.Name == "Other User");
    }

    [Fact]
    public async Task UpdateStrategy_WhenStrategyBelongsToAnotherUser_ReturnsNotFound()
    {
        var (dbContext, repository) = CreateRepository();
        var strategy = CreateStrategy(Guid.NewGuid(), name: "Other");
        dbContext.Strategy.Add(strategy);
        await dbContext.SaveChangesAsync();

        var handler = new UpdateStrategyCommandHandler(repository);
        var request = WithUser(
            new UpdateStrategyCommand(
                strategy.Id,
                new UpdateStrategyCommand.UpdateStrategyCommandModel
                {
                    Name = "Updated",
                    MarketType = MarketType.Crypto
                }),
            Guid.NewGuid());

        var response = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(OperationResult.NotFound, response.Result);
        Assert.Equal("Other", (await dbContext.Strategy.SingleAsync()).Name);
    }

    [Fact]
    public async Task HardDeleteStrategy_WhenStrategyIsActive_ReturnsBadRequest()
    {
        var (dbContext, repository) = CreateRepository();
        var userId = Guid.NewGuid();
        var strategy = CreateStrategy(userId, isActive: true);
        dbContext.Strategy.Add(strategy);
        await dbContext.SaveChangesAsync();

        var response = await new HardDeleteStrategyCommandHandler(repository)
            .Handle(
                WithUser(new HardDeleteStrategyCommand(strategy.Id), userId),
                CancellationToken.None);

        Assert.Equal(OperationResult.BadRequest, response.Result);
        Assert.Equal(1, await dbContext.Strategy.CountAsync());
    }

    [Fact]
    public async Task HardDeleteStrategy_WhenStrategyHasRules_ReturnsBadRequest()
    {
        var (dbContext, repository) = CreateRepository();
        var userId = Guid.NewGuid();
        var strategy = CreateStrategy(userId, isActive: false);
        dbContext.Strategy.Add(strategy);
        dbContext.StrategyRule.Add(CreateStrategyRule(strategy.Id, isActive: false));
        await dbContext.SaveChangesAsync();

        var response = await new HardDeleteStrategyCommandHandler(repository)
            .Handle(
                WithUser(new HardDeleteStrategyCommand(strategy.Id), userId),
                CancellationToken.None);

        Assert.Equal(OperationResult.BadRequest, response.Result);
        Assert.Equal(1, await dbContext.Strategy.CountAsync());
    }

    [Fact]
    public async Task CreateStrategyRule_WhenStrategyBelongsToAnotherUser_ReturnsNotFound()
    {
        var (dbContext, repository) = CreateRepository();
        var strategy = CreateStrategy(Guid.NewGuid());
        dbContext.Strategy.Add(strategy);
        await dbContext.SaveChangesAsync();

        var handler = new CreateStrategyRuleCommandHandler(repository);
        var request = WithUser(
            new CreateStrategyRuleCommand(
                strategy.Id,
                new CreateStrategyRuleCommand.CreateStrategyRuleCommandModel
                {
                    Title = "Risk",
                    Description = "Max 1%",
                    Order = 1,
                    IsRequired = true
                }),
            Guid.NewGuid());

        var response = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(OperationResult.NotFound, response.Result);
        Assert.Empty(await dbContext.StrategyRule.ToListAsync());
    }

    [Fact]
    public async Task StrategyRuleLifecycle_UsesParentStrategyOwnership()
    {
        var (dbContext, repository) = CreateRepository();
        var userId = Guid.NewGuid();
        var strategy = CreateStrategy(userId);
        dbContext.Strategy.Add(strategy);
        await dbContext.SaveChangesAsync();

        var createResponse = await new CreateStrategyRuleCommandHandler(repository)
            .Handle(
                WithUser(
                    new CreateStrategyRuleCommand(
                        strategy.Id,
                        new CreateStrategyRuleCommand.CreateStrategyRuleCommandModel
                        {
                            Title = " Entry ",
                            Description = " Wait for close ",
                            Order = 1,
                            IsRequired = true,
                            Category = StrategyRuleCategory.Entry,
                            Importance = StrategyRuleImportance.High
                        }),
                    userId),
                CancellationToken.None);

        var rule = await dbContext.StrategyRule.SingleAsync();
        Assert.Equal(OperationResult.Created, createResponse.Result);
        Assert.Equal("Entry", rule.Title);
        Assert.Equal("Wait for close", rule.Description);
        Assert.True(rule.IsRequired);
        Assert.Equal(StrategyRuleCategory.Entry, rule.Category);
        Assert.Equal(StrategyRuleImportance.High, rule.Importance);

        var updateResponse = await new UpdateStrategyRuleCommandHandler(repository)
            .Handle(
                WithUser(
                    new UpdateStrategyRuleCommand(
                        strategy.Id,
                        rule.Id,
                        new UpdateStrategyRuleCommand.UpdateStrategyRuleCommandModel
                        {
                            Title = "Updated",
                            Order = 2,
                            IsRequired = false,
                            Category = StrategyRuleCategory.Exit,
                            Importance = StrategyRuleImportance.Critical
                        }),
                    userId),
                CancellationToken.None);

        Assert.Equal(OperationResult.Updated, updateResponse.Result);
        var updatedRule = await dbContext.StrategyRule.SingleAsync();
        Assert.Equal("Updated", updatedRule.Title);
        Assert.Equal(StrategyRuleCategory.Exit, updatedRule.Category);
        Assert.Equal(StrategyRuleImportance.Critical, updatedRule.Importance);

        var deleteResponse = await new SoftDeleteStrategyRuleCommandHandler(repository)
            .Handle(
                WithUser(new SoftDeleteStrategyRuleCommand(strategy.Id, rule.Id), userId),
                CancellationToken.None);

        Assert.Equal(OperationResult.Deleted, deleteResponse.Result);
        Assert.False((await dbContext.StrategyRule.SingleAsync()).IsActive);
    }

    [Fact]
    public async Task HardDeleteStrategyRule_WhenRuleIsActive_ReturnsBadRequest()
    {
        var (dbContext, repository) = CreateRepository();
        var userId = Guid.NewGuid();
        var strategy = CreateStrategy(userId);
        var rule = CreateStrategyRule(strategy.Id, isActive: true);
        dbContext.Strategy.Add(strategy);
        dbContext.StrategyRule.Add(rule);
        await dbContext.SaveChangesAsync();

        var response = await new HardDeleteStrategyRuleCommandHandler(repository)
            .Handle(
                WithUser(new HardDeleteStrategyRuleCommand(strategy.Id, rule.Id), userId),
                CancellationToken.None);

        Assert.Equal(OperationResult.BadRequest, response.Result);
        Assert.Equal(1, await dbContext.StrategyRule.CountAsync());
    }

    [Fact]
    public async Task HardDeleteStrategyRule_WhenStrategyBelongsToAnotherUser_ReturnsNotFound()
    {
        var (dbContext, repository) = CreateRepository();
        var strategy = CreateStrategy(Guid.NewGuid());
        var rule = CreateStrategyRule(strategy.Id, isActive: false);
        dbContext.Strategy.Add(strategy);
        dbContext.StrategyRule.Add(rule);
        await dbContext.SaveChangesAsync();

        var response = await new HardDeleteStrategyRuleCommandHandler(repository)
            .Handle(
                WithUser(new HardDeleteStrategyRuleCommand(strategy.Id, rule.Id), Guid.NewGuid()),
                CancellationToken.None);

        Assert.Equal(OperationResult.NotFound, response.Result);
        Assert.Equal(1, await dbContext.StrategyRule.CountAsync());
    }

    private static TRequest WithUser<TRequest>(TRequest request, Guid userId)
        where TRequest : IContextualRequest
    {
        request.Context.Add(ContextKeys.UserId, userId);
        request.Context.Add(ContextKeys.ExternalUserId, $"idp|{userId}");
        return request;
    }

    private static Strategy CreateStrategy(
        Guid userId,
        string name = "Strategy",
        bool isActive = true)
        => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Description = "Description",
            MarketType = MarketType.Forex,
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = userId,
            ModifiedByUserId = userId
        };

    private static StrategyRule CreateStrategyRule(
        Guid strategyId,
        bool isActive = true)
        => new()
        {
            Id = Guid.NewGuid(),
            StrategyId = strategyId,
            Title = "Rule",
            Description = "Description",
            Order = 1,
            IsRequired = true,
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow,
            ModifiedAt = DateTimeOffset.UtcNow
        };

    private static (TradeXDbContext DbContext, TradeXRepository Repository) CreateRepository()
    {
        var options = new DbContextOptionsBuilder<TradeXDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContext = new TradeXDbContext(options);
        var repository = new TradeXRepository(
            dbContext,
            NullLogger<RepositoryService<TradeXDbContext>>.Instance);

        return (dbContext, repository);
    }
}
