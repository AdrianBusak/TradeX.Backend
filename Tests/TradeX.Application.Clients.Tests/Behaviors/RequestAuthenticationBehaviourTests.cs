using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TradeX.Application.Abstractions.Behaviors;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Abstractions.Services;
using TradeX.Domain.Entities;
using TradeX.Repository;
using TradeX.Repository.Services;

namespace TradeX.Application.Clients.Tests.Behaviors;

public class RequestAuthenticationBehaviourTests
{
    [Fact]
    public async Task Handle_WhenTokenIsMissing_ReturnsUnauthorized()
    {
        var (_, repository) = CreateRepository();
        var behavior = CreateBehavior(
            new AuthenticatedUserContext(null, null, null, null, true),
            repository);
        var request = new TestAuthenticatedRequest();

        var response = await behavior.Handle(
            request,
            ThrowingNext,
            CancellationToken.None);

        Assert.Equal(OperationResult.Unauthorized, response.Result);
    }

    [Fact]
    public async Task Handle_WhenTokenUserIsInactive_ReturnsForbidden()
    {
        var (_, repository) = CreateRepository();
        var behavior = CreateBehavior(
            new AuthenticatedUserContext("idp|inactive", "inactive@example.com", null, null, false),
            repository);
        var request = new TestAuthenticatedRequest();

        var response = await behavior.Handle(
            request,
            ThrowingNext,
            CancellationToken.None);

        Assert.Equal(OperationResult.Forbidden, response.Result);
    }

    [Fact]
    public async Task Handle_WhenEmailClaimIsMissing_ReturnsUnauthorizedWithoutCreatingUser()
    {
        var (dbContext, repository) = CreateRepository();
        var behavior = CreateBehavior(
            new AuthenticatedUserContext("idp|no-email", null, "No", "Email", true),
            repository);
        var request = new TestAuthenticatedRequest();

        var response = await behavior.Handle(
            request,
            ThrowingNext,
            CancellationToken.None);

        Assert.Equal(OperationResult.Unauthorized, response.Result);
        Assert.Empty(await dbContext.User.ToListAsync());
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_CreatesUserAndPopulatesRequestContext()
    {
        var (dbContext, repository) = CreateRepository();
        var behavior = CreateBehavior(
            new AuthenticatedUserContext("idp|new", " new@example.com ", " New ", " User ", true),
            repository);
        var request = new TestAuthenticatedRequest();
        var nextCalled = false;

        var response = await behavior.Handle(
            request,
            () =>
            {
                nextCalled = true;
                return Task.FromResult(new StandardResponse<object>(OperationResult.Ok, new object()));
            },
            CancellationToken.None);

        var user = await dbContext.User.SingleAsync();
        Assert.True(nextCalled);
        Assert.Equal(OperationResult.Ok, response.Result);
        Assert.Equal("idp|new", request.ExternalUserId());
        Assert.Equal(user.Id, request.UserId());
        Assert.Equal("idp|new", user.ExternalId);
        Assert.Equal("new@example.com", user.Email);
        Assert.Equal("New", user.FirstName);
        Assert.Equal("User", user.LastName);
        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task Handle_WhenUserExists_UpdatesChangedProfileClaims()
    {
        var (dbContext, repository) = CreateRepository();
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            ExternalId = "idp|existing",
            Email = "old@example.com",
            FirstName = "Old",
            LastName = "Name",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            ModifiedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };
        dbContext.User.Add(existingUser);
        await dbContext.SaveChangesAsync();

        var behavior = CreateBehavior(
            new AuthenticatedUserContext("idp|existing", "new@example.com", "New", "Name", true),
            repository);
        var request = new TestAuthenticatedRequest();

        var response = await behavior.Handle(
            request,
            () => Task.FromResult(new StandardResponse<object>(OperationResult.Ok, new object())),
            CancellationToken.None);

        var user = await dbContext.User.SingleAsync();
        Assert.Equal(OperationResult.Ok, response.Result);
        Assert.Equal(existingUser.Id, request.UserId());
        Assert.Equal("new@example.com", user.Email);
        Assert.Equal("New", user.FirstName);
        Assert.Equal("Name", user.LastName);
    }

    private static RequestAuthenticationBehaviour<TestAuthenticatedRequest, StandardResponse<object>> CreateBehavior(
        AuthenticatedUserContext userContext,
        ITradeXRepository repository)
        => new(
            new StubUserContextAccessor(userContext),
            repository,
            NullLogger<TestAuthenticatedRequest>.Instance);

    private static (TradeXDbContext DbContext, ITradeXRepository Repository) CreateRepository()
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

    private static Task<StandardResponse<object>> ThrowingNext()
        => throw new InvalidOperationException("Next should not be called.");

    private sealed class StubUserContextAccessor(AuthenticatedUserContext userContext) : IUserContextAccessor
    {
        public Task<AuthenticatedUserContext> GetAuthenticatedUserAsync()
            => Task.FromResult(userContext);
    }

    private sealed class TestAuthenticatedRequest
        : ContextualRequest, IRequest<StandardResponse<object>>, IAuthenticatedRequest
    {
    }
}
