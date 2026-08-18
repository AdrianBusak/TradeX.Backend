using Microsoft.ML;
using TradeX.Application.Abstractions.Configuration;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Clients.Features.MachineLearning;
using TradeX.Domain.Entities;
using TradeX.Infrastructure.MachineLearning.Models;

namespace TradeX.Infrastructure.MachineLearning;

internal sealed class TradeOutcomeMlService(
    ITradeXRepository repository,
    ITradeOutcomeDatasetBuilder datasetBuilder,
    TradeOutcomeFeatureBuilder featureBuilder,
    ITradeOutcomeModelStorage modelStorage,
    MLContext mlContext,
    ApplicationConfiguration applicationConfiguration)
    : ITradeOutcomeMlService
{
    private readonly TradeOutcomeMlConfiguration _configuration =
        applicationConfiguration.TradeOutcomeMlConfiguration ?? new TradeOutcomeMlConfiguration();

    public async Task<PreTradeMlReadinessResponse> GetReadinessAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var rows = await datasetBuilder.BuildAsync(userId, cancellationToken).ConfigureAwait(false);
        var activeModel = await GetActiveModelAsync(userId, cancellationToken).ConfigureAwait(false);

        return CreateReadiness(rows, activeModel is not null);
    }

    public async Task<TrainPreTradeMlModelResponse> TrainAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var rows = await datasetBuilder.BuildAsync(userId, cancellationToken).ConfigureAwait(false);
        var readiness = CreateReadiness(rows, hasActiveModel: false);

        if (!readiness.IsReady)
        {
            return new TrainPreTradeMlModelResponse
            {
                IsReady = false,
                SampleCount = rows.Count,
                PositiveCount = readiness.PositiveCount,
                NonPositiveCount = readiness.NonPositiveCount,
                Reason = readiness.Reason
            };
        }

        var data = mlContext.Data.LoadFromEnumerable(rows);
        var model = CreatePipeline(mlContext).Fit(data);
        var modelVersion = $"v2-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        var modelPath = await modelStorage.SaveAsync(
                userId,
                modelVersion,
                model,
                data.Schema,
                cancellationToken)
            .ConfigureAwait(false);
        var trainedAt = DateTime.UtcNow;

        await SaveModelRegistryAsync(
                userId,
                modelVersion,
                modelPath,
                rows,
                trainedAt,
                cancellationToken)
            .ConfigureAwait(false);

        return new TrainPreTradeMlModelResponse
        {
            IsReady = true,
            ModelVersion = modelVersion,
            SampleCount = rows.Count,
            PositiveCount = rows.Count(row => row.Label),
            NonPositiveCount = rows.Count(row => !row.Label),
            TrainedAt = trainedAt
        };
    }

    public async Task<PreTradeScoreResponse> ScoreAsync(
        Guid userId,
        PreTradeScoreRequest request,
        CancellationToken cancellationToken)
    {
        var registry = await GetActiveModelAsync(userId, cancellationToken).ConfigureAwait(false);

        if (registry is null)
        {
            return NotReady("Train a model after you have enough valid closed trades.");
        }

        var instrument = await repository.GetSingleAsync<TradingInstrument>(
                entity => entity.Id == request.TradingInstrumentId &&
                          entity.UserId == userId &&
                          entity.IsActive,
                cancellationToken)
            .ConfigureAwait(false);

        if (instrument is null)
        {
            return NotReady("Trading instrument was not found.");
        }

        var model = await modelStorage.LoadAsync(registry.ModelPath, cancellationToken)
            .ConfigureAwait(false);
        var engine = mlContext.Model
            .CreatePredictionEngine<TradeOutcomeTrainingRow, TradeOutcomePrediction>(model);
        var prediction = engine.Predict(featureBuilder.CreateFromScoreRequest(request, instrument));

        return new PreTradeScoreResponse
        {
            IsReady = true,
            PositiveOutcomeProbability = Math.Clamp((decimal)prediction.Probability, 0, 1),
            SampleCount = registry.SampleCount,
            ModelVersion = registry.ModelVersion,
            TrainedAt = registry.TrainedAt,
            Confidence = GetConfidence(registry.SampleCount),
            Message = "Estimated probability that Effective R will be positive."
        };
    }

    private async Task<UserTradeOutcomeModel?> GetActiveModelAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await repository.GetSingleAsync<UserTradeOutcomeModel>(
                entity => entity.UserId == userId && entity.IsActiveModel,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task SaveModelRegistryAsync(
        Guid userId,
        string modelVersion,
        string modelPath,
        List<TradeOutcomeTrainingRow> rows,
        DateTime trainedAt,
        CancellationToken cancellationToken)
    {
        var activeModels = await repository.GetListAsync<UserTradeOutcomeModel>(
                cancellationToken,
                entity => entity.UserId == userId && entity.IsActiveModel)
            .ConfigureAwait(false);

        foreach (var activeModel in activeModels)
        {
            activeModel.IsActiveModel = false;
            activeModel.ModifiedByUserId = userId;
        }

        if (activeModels.Count > 0)
        {
            await repository.UpdateRangeAsync(activeModels, cancellationToken).ConfigureAwait(false);
        }

        await repository.AddAsync(new UserTradeOutcomeModel
        {
            UserId = userId,
            ModelVersion = modelVersion,
            ModelPath = modelPath,
            SampleCount = rows.Count,
            PositiveCount = rows.Count(row => row.Label),
            NonPositiveCount = rows.Count(row => !row.Label),
            TrainedAt = trainedAt,
            FeatureSchemaVersion = _configuration.FeatureSchemaVersion,
            IsActiveModel = true,
            IsActive = true,
            CreatedByUserId = userId,
            ModifiedByUserId = userId
        }, cancellationToken).ConfigureAwait(false);
    }

    private static IEstimator<ITransformer> CreatePipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Categorical.OneHotEncoding(
                "StrategyEncoded",
                nameof(TradeOutcomeTrainingRow.StrategyId))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding(
                "SymbolEncoded",
                nameof(TradeOutcomeTrainingRow.Symbol)))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding(
                "MarketTypeEncoded",
                nameof(TradeOutcomeTrainingRow.MarketType)))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding(
                "DirectionEncoded",
                nameof(TradeOutcomeTrainingRow.Direction)))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding(
                "SessionEncoded",
                nameof(TradeOutcomeTrainingRow.Session)))
            .Append(mlContext.Transforms.Concatenate(
                "Features",
                "StrategyEncoded",
                "SymbolEncoded",
                "MarketTypeEncoded",
                "DirectionEncoded",
                "SessionEncoded",
                nameof(TradeOutcomeTrainingRow.DayOfWeek),
                nameof(TradeOutcomeTrainingRow.Hour),
                nameof(TradeOutcomeTrainingRow.PlannedRiskReward),
                nameof(TradeOutcomeTrainingRow.StopLossDistance),
                nameof(TradeOutcomeTrainingRow.RuleCheckCount),
                nameof(TradeOutcomeTrainingRow.FollowedRuleCheckCount),
                nameof(TradeOutcomeTrainingRow.RuleCompliancePercent),
                nameof(TradeOutcomeTrainingRow.RiskAmount)))
            .Append(mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression());
    }

    private PreTradeMlReadinessResponse CreateReadiness(
        List<TradeOutcomeTrainingRow> rows,
        bool hasActiveModel)
    {
        var positiveCount = rows.Count(row => row.Label);
        var nonPositiveCount = rows.Count - positiveCount;
        var isReady = rows.Count >= _configuration.MinimumTotalTrades &&
                      positiveCount >= _configuration.MinimumPositiveTrades &&
                      nonPositiveCount >= _configuration.MinimumNonPositiveTrades;

        return new PreTradeMlReadinessResponse
        {
            IsReady = isReady,
            ClosedTradeCount = rows.Count,
            PositiveCount = positiveCount,
            NonPositiveCount = nonPositiveCount,
            MinimumRequired = _configuration.MinimumTotalTrades,
            MinimumPositiveRequired = _configuration.MinimumPositiveTrades,
            MinimumNonPositiveRequired = _configuration.MinimumNonPositiveTrades,
            HasActiveModel = hasActiveModel,
            Reason = isReady
                ? null
                : $"At least {_configuration.MinimumTotalTrades} valid closed trades, " +
                  $"{_configuration.MinimumPositiveTrades} positive and " +
                  $"{_configuration.MinimumNonPositiveTrades} non-positive trades are required."
        };
    }

    private static string GetConfidence(int sampleCount)
    {
        return sampleCount < 150
            ? "Low"
            : sampleCount < 300
                ? "Medium"
                : "High";
    }

    private static PreTradeScoreResponse NotReady(string message)
    {
        return new PreTradeScoreResponse
        {
            IsReady = false,
            Message = message
        };
    }
}
