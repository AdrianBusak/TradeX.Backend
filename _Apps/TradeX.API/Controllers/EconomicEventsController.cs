using API.Abstractions.Controllers;
using API.Abstractions.Extensions;
using API.Abstractions.Interfaces;
using API.Abstractions.OpenApi;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.QueryParameters;
using TradeX.Application.Clients.Features.EconomicCalendar.Queries;

namespace TradeX.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public sealed class EconomicEventsController(
    ILogger<EconomicEventsController> logger,
    IMediator mediator,
    IHttpRequestProcessingService httpRequestProcessor)
    : BaseController<EconomicEventsController>(logger, mediator, httpRequestProcessor)
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Get(
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery, SwaggerFilterDescription] string? filters,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            var result = await Mediator.Send(
                    new GetEconomicEventsQuery
                    {
                        From = from,
                        To = to,
                        FilterParameters = filters?.GetQueryParameter<FilterQueryParameters>()
                    },
                    cancellationToken)
                .ConfigureAwait(false);
            return (IStandardResponse)result;
        }, Logger).ConfigureAwait(false);
    }
}
