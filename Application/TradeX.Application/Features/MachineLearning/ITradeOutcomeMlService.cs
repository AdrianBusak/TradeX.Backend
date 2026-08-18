namespace TradeX.Application.Clients.Features.MachineLearning;

public interface ITradeOutcomeMlService
{
    Task<PreTradeMlReadinessResponse> GetReadinessAsync(Guid userId, CancellationToken cancellationToken);
    Task<TrainPreTradeMlModelResponse> TrainAsync(Guid userId, CancellationToken cancellationToken);
    Task<PreTradeScoreResponse> ScoreAsync(Guid userId, PreTradeScoreRequest request, CancellationToken cancellationToken);
}
