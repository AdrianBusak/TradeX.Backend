using Microsoft.ML;
using Microsoft.ML.Data;

namespace TradeX.Infrastructure.MachineLearning;

internal interface ITradeOutcomeModelStorage
{
    Task<string> SaveAsync(
        Guid userId,
        string modelVersion,
        ITransformer model,
        DataViewSchema schema,
        CancellationToken cancellationToken);

    Task<ITransformer> LoadAsync(string modelPath, CancellationToken cancellationToken);
}
