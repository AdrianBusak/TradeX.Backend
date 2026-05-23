using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TradeX.Application.Abstractions.Constants;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Services;
using TradeX.Application.Clients.Features.Users.Queries;
using TradeX.Domain.Entities;
using TradeX.Repository;
using TradeX.Repository.Services;

namespace TradeX.Application.Clients.Tests.Features.Users.Queries;

public class GetMyProfileQueryTests
{
    [Fact]
    public async Task Handle_WhenUserExists_ReturnsMyProfile()
    {
        var (dbContext, repository) = CreateRepository();
        var user = new User
        {
            Id = Guid.NewGuid(),
            ExternalId = "auth0|user",
            Email = "user@example.com",
            FirstName = "Test",
            LastName = "User",
            IsActive = true
        };
        dbContext.User.Add(user);
        await dbContext.SaveChangesAsync();

        var request = CreateRequest(user.Id, user.ExternalId);
        var handler = new GetMyProfileQueryHandler(repository);

        var response = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(OperationResult.Ok, response.Result);
        Assert.NotNull(response.Model);
        Assert.Equal(user.Id, response.Model.Id);
        Assert.Equal("auth0|user", response.Model.ExternalId);
        Assert.Equal("user@example.com", response.Model.Email);
        Assert.Equal("Test", response.Model.FirstName);
        Assert.Equal("User", response.Model.LastName);
        Assert.True(response.Model.IsActive);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ReturnsNotFound()
    {
        var (_, repository) = CreateRepository();
        var userId = Guid.NewGuid();
        var request = CreateRequest(userId, "auth0|missing");
        var handler = new GetMyProfileQueryHandler(repository);

        var response = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(OperationResult.NotFound, response.Result);
        Assert.Null(response.Model);
    }

    private static GetMyProfileQuery CreateRequest(Guid userId, string externalUserId)
    {
        var request = new GetMyProfileQuery();
        request.Context.Add(ContextKeys.UserId, userId);
        request.Context.Add(ContextKeys.ExternalUserId, externalUserId);
        return request;
    }

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
