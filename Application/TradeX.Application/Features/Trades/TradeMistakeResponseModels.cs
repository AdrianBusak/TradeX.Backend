using Microsoft.EntityFrameworkCore;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Trades;

public sealed class GetTradeMistakesResponse
{
    public Guid TradeId { get; set; }
    public int TotalMistakes { get; set; }
    public List<TradeMistakeItemResponse> Mistakes { get; set; } = [];
}

public sealed class TradeMistakeItemResponse
{
    public Guid MistakeId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Note { get; set; }
}

internal static class TradeMistakeResponseFactory
{
    public static async Task<GetTradeMistakesResponse> CreateAsync(
        ITradeXRepository repository,
        Trade trade,
        CancellationToken cancellationToken)
    {
        var data = await repository.QueryAsync(
            from tradeMistake in repository.DbContext.TradeMistake.IgnoreQueryFilters()
            join mistake in repository.DbContext.Mistake.IgnoreQueryFilters()
                on tradeMistake.MistakeId equals mistake.Id
            where tradeMistake.TradeId == trade.Id
            orderby mistake.Name
            select new TradeMistakeItemResponse
            {
                MistakeId = mistake.Id,
                Name = mistake.Name,
                Description = mistake.Description,
                Note = tradeMistake.Note
            },
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var mistakes = data.Records ?? [];

        return new GetTradeMistakesResponse
        {
            TradeId = trade.Id,
            TotalMistakes = mistakes.Count,
            Mistakes = mistakes
        };
    }
}
