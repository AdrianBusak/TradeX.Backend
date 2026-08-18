using Microsoft.ML.Data;

namespace TradeX.Infrastructure.MachineLearning.Models;

internal sealed class TradeOutcomePrediction
{
    [ColumnName("PredictedLabel")]
    public bool PredictedLabel { get; set; }

    public float Probability { get; set; }
    public float Score { get; set; }
}
