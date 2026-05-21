using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Abstractions.Behaviors;

public sealed class RequestAuthenticationBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IAuthenticatedRequest
    where TResponse : IStandardResponse
{
    private static readonly Type[] ResponseCtorSignature =
    [
        typeof(OperationResult),
        typeof(string),
        typeof(object)
    ];

    private readonly IUserContextAccessor _userContext;
    private readonly ITradeXRepository _repository;
    private readonly ILogger<TRequest> _logger;

    public RequestAuthenticationBehaviour(
        IUserContextAccessor userContext,
        ITradeXRepository repository,
        ILogger<TRequest> logger)
    {
        _userContext = userContext;
        _repository = repository;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            var authResult = await ResolveUserAsync(cancellationToken).ConfigureAwait(false);
            if (authResult.Response is not null)
                return authResult.Response;

            PopulateRequestContext(request, authResult);
        }
        catch (SecurityTokenExpiredException ex)
        {
            return CreateExpiredTokenResponse(ex);
        }
        catch (SecurityTokenSignatureKeyNotFoundException)
        {
            return Unauthorized("Token signature not valid. Please check provided Bearer token.");
        }
        catch (Exception ex)
        {
            return Unauthorized(
                $"Error reading user identifier. Please check provided Bearer token. [Exception: {ex.Message}]",
                ex);
        }

        return await next().ConfigureAwait(false);
    }

    private async Task<AuthResolutionResult> ResolveUserAsync(CancellationToken ct)
    {
        var (externalUserId, isActive) = await _userContext.GetUserIdentifierAsync().ConfigureAwait(false);

        if (externalUserId is null)
            return AuthResolutionResult.Fail(
                MissingToken($"Bearer token not provided or invalid format. [RequestName: {typeof(TRequest).Name}]"));

        if (!isActive)
            return AuthResolutionResult.Fail(
                Forbidden($"User not active. [ExternalId: {externalUserId}] [RequestName: {typeof(TRequest).Name}]"));

        var userId = await EnsureUserExistsAsync(externalUserId, ct).ConfigureAwait(false);

        return new AuthResolutionResult(userId, externalUserId);
    }

    private async Task<Guid> EnsureUserExistsAsync(string externalUserId, CancellationToken cancellationToken)
    {
        var existingUserId = await _repository.DbContext.User
            .Where(u => u.ExternalId == externalUserId)
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (existingUserId.HasValue)
            return existingUserId.Value;

        var user = new User
        {
            ExternalId = externalUserId
        };

        return await _repository.AddAsync(user, cancellationToken).ConfigureAwait(false);
    }

    private static void PopulateRequestContext(TRequest request, AuthResolutionResult auth)
    {
        request.Context.Add(Constants.ContextKeys.UserId, auth.UserId);
        request.Context.Add(Constants.ContextKeys.ExternalUserId, auth.ExternalUserId);
    }

    private TResponse Forbidden(string message)
        => CreateResponse(OperationResult.Forbidden, message);

    private TResponse Unauthorized(string message, Exception? ex = null)
        => CreateResponse(OperationResult.Unauthorized, message, ex);

    private TResponse MissingToken(string message)
        => CreateResponse(OperationResult.Unauthorized, message);

    private TResponse CreateExpiredTokenResponse(SecurityTokenExpiredException ex)
        => CreateResponse(
            OperationResult.Unauthorized,
            OperationResult.Unauthorized.ToString(),
            new TokenExpiredResponseModel { Message = ex.Message });

    private TResponse CreateResponse(
        OperationResult result,
        string message,
        object? payload = null,
        Exception? ex = null)
    {
        if (ex is not null)
        {
            _logger.LogError(
                ex,
                "Error in {RequestName}. [Error: {Error}]",
                typeof(TRequest).Name,
                ex.ToDeepString());
        }

        var ctor = typeof(TResponse).GetConstructor(ResponseCtorSignature)
            ?? throw new InvalidOperationException(
                $"Cannot construct {typeof(TResponse).Name}. " +
                "Expected ctor (OperationResult, string, object).");

        return (TResponse)ctor.Invoke(
        [
            result,
            message,
            payload ?? new { ErrorMessage = message }
        ]);
    }

    private sealed record AuthResolutionResult(Guid UserId, string ExternalUserId)
    {
        public TResponse? Response { get; init; }

        public static AuthResolutionResult Fail(TResponse response)
            => new(default, string.Empty) { Response = response };
    }
}
