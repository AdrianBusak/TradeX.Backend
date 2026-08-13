using MediatR;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Application.Clients.Features.Mistakes;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Mistakes.Queries;

public sealed class GetMistakesQuery : ContextualRequest, IRequest<StandardListResponse<MistakeResponse>>, IAuthenticatedRequest;

public sealed class GetMistakesQueryHandler(ITradeXRepository repository) : IRequestHandler<GetMistakesQuery, StandardListResponse<MistakeResponse>>
{
    public async Task<StandardListResponse<MistakeResponse>> Handle(GetMistakesQuery request, CancellationToken cancellationToken)
    {
        var records = await repository.QueryAsync(
                from mistake in repository.DbContext.Mistake
                where mistake.UserId == request.UserId() && mistake.IsActive
                orderby mistake.Name
                select new MistakeResponse
                {
                    Id = mistake.Id,
                    Name = mistake.Name,
                    Description = mistake.Description,
                    IsActive = mistake.IsActive
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return new StandardListResponse<MistakeResponse>(records.Records!, records.TotalRecordCount, records.PageIndex, records.PageSize);
    }
}
