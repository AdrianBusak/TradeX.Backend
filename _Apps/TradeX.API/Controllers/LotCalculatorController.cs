using API.Abstractions.Controllers;
using API.Abstractions.Interfaces;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Clients.Features.LotCalculator.Commands;

namespace TradeX.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/lot-calculator")]
public sealed class LotCalculatorController(
    ILogger<LotCalculatorController> logger,
    IMediator mediator,
    IHttpRequestProcessingService httpRequestProcessor)
    : BaseController<LotCalculatorController>(logger, mediator, httpRequestProcessor)
{
    [HttpPost("calculate")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Calculate(
        [FromBody] CalculateLotRequest model,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
            (IStandardResponse)await Mediator.Send(new CalculateLotCommand(model), cancellationToken)
                .ConfigureAwait(false), Logger).ConfigureAwait(false);
    }
}
