using TradeX.Domain.Abstractions.Entities;

namespace TradeX.Domain.Entities;

public partial class UserTradeOutcomeModel : BaseEntity
{
    public Guid UserId { get; set; }
    public string ModelVersion { get; set; } = null!;
    public string ModelPath { get; set; } = null!;
    public int SampleCount { get; set; }
    public int PositiveCount { get; set; }
    public int NonPositiveCount { get; set; }
    public DateTime TrainedAt { get; set; }
    public string FeatureSchemaVersion { get; set; } = "v1";
    public bool IsActiveModel { get; set; } = true;
}
