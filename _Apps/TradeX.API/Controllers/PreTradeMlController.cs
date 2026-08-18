using API.Abstractions.Controllers;
using API.Abstractions.Interfaces;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Clients.Features.MachineLearning;
using TradeX.Application.Clients.Features.MachineLearning.Commands;
using TradeX.Application.Clients.Features.MachineLearning.Queries;

namespace TradeX.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/ml/pre-trade")]
public class PreTradeMlController(
    ILogger<PreTradeMlController> logger,
    IMediator mediator,
    IHttpRequestProcessingService httpRequestProcessor)
    : BaseController<PreTradeMlController>(logger, mediator, httpRequestProcessor)
{
    [HttpGet("readiness")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> GetReadiness(CancellationToken cancellationToken)
        => await ProcessAsync(new GetPreTradeMlReadinessQuery(), cancellationToken).ConfigureAwait(false);

    [HttpPost("train")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Train(CancellationToken cancellationToken)
        => await ProcessAsync(new TrainPreTradeMlModelCommand(), cancellationToken).ConfigureAwait(false);

    [HttpPost("score")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Score(
        [FromBody] PreTradeScoreRequest model,
        CancellationToken cancellationToken)
        => await ProcessAsync(new ScorePreTradeOutcomeCommand(model), cancellationToken)
            .ConfigureAwait(false);

    private Task<ContentResult> ProcessAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken)
        where TResponse : IStandardResponse
    {
        return HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(request, cancellationToken).ConfigureAwait(false);
        }, Logger);
    }
}
