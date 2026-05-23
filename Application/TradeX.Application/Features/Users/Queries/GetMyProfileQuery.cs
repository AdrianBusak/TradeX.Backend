using MediatR;
using Microsoft.EntityFrameworkCore;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Users.Queries;

public sealed class GetMyProfileQuery
    : ContextualRequest, IRequest<StandardResponse<GetMyProfileResponse>>, IAuthenticatedRequest
{
}

public sealed class GetMyProfileQueryHandler(ITradeXRepository repository)
    : IRequestHandler<GetMyProfileQuery, StandardResponse<GetMyProfileResponse>>
{
    public async Task<StandardResponse<GetMyProfileResponse>> Handle(
        GetMyProfileQuery request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        var user = await repository.DbContext.User
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new GetMyProfileResponse
            {
                Id = u.Id,
                ExternalId = u.ExternalId,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsActive = u.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return StandardResponseFactory
                .CreateEntityNotFoundStandardResponse<GetMyProfileResponse>(userId, nameof(User));
        }

        return new StandardResponse<GetMyProfileResponse>(OperationResult.Ok, user);
    }
}

public sealed class GetMyProfileResponse
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = null!;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; }
}
