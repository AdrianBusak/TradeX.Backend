using TradeX.Application.Abstractions.Interfaces;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Trades;

public sealed class GetTradeRuleChecksResponse
{
    public Guid TradeId { get; set; }
    public Guid StrategyId { get; set; }
    public decimal? ComplianceScore { get; set; }
    public int TotalRules { get; set; }
    public int CheckedRules { get; set; }
    public int FollowedRules { get; set; }
    public int BrokenRules { get; set; }
    public List<TradeRuleCheckItemResponse> Rules { get; set; } = [];
}

public sealed class TradeRuleCheckItemResponse
{
    public Guid StrategyRuleId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public bool? IsFollowed { get; set; }
    public string? Note { get; set; }
}

internal static class TradeRuleCheckResponseFactory
{
    public static async Task<GetTradeRuleChecksResponse> CreateAsync(
        ITradeXRepository repository,
        Trade trade,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var rulesData = await repository.QueryAsync(
                from rule in repository.DbContext.StrategyRule
                where rule.StrategyId == trade.StrategyId && rule.IsActive
                orderby rule.Order, rule.Id
                select new RuleRow
                {
                    Id = rule.Id,
                    Title = rule.Title,
                    Description = rule.Description,
                    Order = rule.Order,
                    IsRequired = rule.IsRequired
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var rules = rulesData.Records ?? [];
        var ruleIds = rules.Select(x => x.Id).ToList();
        var checks = ruleIds.Count == 0
            ? []
            : await repository.GetListAsync<TradeRuleCheck>(
                    cancellationToken,
                    check => check.TradeId == trade.Id &&
                             check.UserId == userId &&
                             check.IsActive &&
                             ruleIds.Contains(check.StrategyRuleId))
                .ConfigureAwait(false);

        var checksByRuleId = checks.ToDictionary(x => x.StrategyRuleId);
        var followedRules = checks.Count(x => x.IsFollowed);
        var checkedRules = checks.Count;

        return new GetTradeRuleChecksResponse
        {
            TradeId = trade.Id,
            StrategyId = trade.StrategyId,
            TotalRules = rules.Count,
            CheckedRules = checkedRules,
            FollowedRules = followedRules,
            BrokenRules = checkedRules - followedRules,
            ComplianceScore = checkedRules == 0 ? null : followedRules * 100m / checkedRules,
            Rules = rules.Select(rule =>
            {
                checksByRuleId.TryGetValue(rule.Id, out var check);
                return new TradeRuleCheckItemResponse
                {
                    StrategyRuleId = rule.Id,
                    Title = rule.Title,
                    Description = rule.Description,
                    Order = rule.Order,
                    IsRequired = rule.IsRequired,
                    IsFollowed = check?.IsFollowed,
                    Note = check?.Note
                };
            }).ToList()
        };
    }

    private sealed class RuleRow
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int Order { get; set; }
        public bool IsRequired { get; set; }
    }
}
