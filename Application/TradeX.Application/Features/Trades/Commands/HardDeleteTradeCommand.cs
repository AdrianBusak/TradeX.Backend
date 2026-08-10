using FluentValidation;
using MediatR;
using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using TradeX.Domain.Entities;

namespace TradeX.Application.Clients.Features.Trades.Commands;

public sealed class HardDeleteTradeCommand(Guid id)
    : ContextualRequest,
      IRequest<StandardResponse<HardDeleteEntityResponseModel>>,
      IAuthenticatedRequest
{
    public Guid Id { get; } = id;

    public sealed class HardDeleteTradeCommandValidator
        : AbstractValidator<HardDeleteTradeCommand>
    {
        public HardDeleteTradeCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}

public sealed class HardDeleteTradeCommandHandler(
    ITradeXRepository repository,
    IBlobStorageService blobStorage)
    : IRequestHandler<HardDeleteTradeCommand, StandardResponse<HardDeleteEntityResponseModel>>
{
    public async Task<StandardResponse<HardDeleteEntityResponseModel>> Handle(
        HardDeleteTradeCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId();

        var trade = await repository.GetSingleAsync<Trade>(
                entity => entity.Id == request.Id && entity.UserId == userId,
                cancellationToken)
            .ConfigureAwait(false);

        if (trade is null)
        {
            return StandardResponseFactory.CreateEntityNotFoundStandardResponse<HardDeleteEntityResponseModel>(
                request.Id,
                nameof(Trade));
        }

        var assignments = await repository.GetListAsync<TradeAccountAssignment>(
                cancellationToken,
                assignment => assignment.TradeId == trade.Id)
            .ConfigureAwait(false);

        if (assignments.Count > 0)
        {
            await repository.DeleteHardRangeAsync<TradeAccountAssignment>(
                    assignments.Select(x => x.Id).ToList(),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        var images = await repository.GetListAsync<TradeImage>(
                cancellationToken,
                image => image.TradeId == trade.Id && image.UserId == userId)
            .ConfigureAwait(false);

        foreach (var image in images)
        {
            await blobStorage.DeleteAsync(image.BlobPath, cancellationToken).ConfigureAwait(false);
        }

        if (images.Count > 0)
        {
            await repository.DeleteHardRangeAsync<TradeImage>(
                    images.Select(x => x.Id).ToList(),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        var ruleChecks = await repository.GetListAsync<TradeRuleCheck>(
                cancellationToken,
                check => check.TradeId == trade.Id && check.UserId == userId)
            .ConfigureAwait(false);

        if (ruleChecks.Count > 0)
        {
            await repository.DeleteHardRangeAsync<TradeRuleCheck>(
                    ruleChecks.Select(x => x.Id).ToList(),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        await repository.DeleteHardAsync<Trade>(trade.Id, cancellationToken)
            .ConfigureAwait(false);

        return new StandardResponse<HardDeleteEntityResponseModel>(
            OperationResult.Deleted,
            new HardDeleteEntityResponseModel());
    }
}
