using TradeX.Infrastructure.MachineLearning.Models;

namespace TradeX.Infrastructure.MachineLearning;

internal interface ITradeOutcomeDatasetBuilder
{
    Task<List<TradeOutcomeTrainingRow>> BuildAsync(Guid userId, CancellationToken cancellationToken);
}
