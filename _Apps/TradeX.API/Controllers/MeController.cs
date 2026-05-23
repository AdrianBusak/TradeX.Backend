using API.Abstractions.Controllers;
using API.Abstractions.Interfaces;
using Asp.Versioning;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Clients.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace TradeX.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public class MeController(
    ILogger<MeController> logger,
    IMediator mediator,
    IHttpRequestProcessingService httpRequestProcessor)
    : BaseController<MeController>(logger, mediator, httpRequestProcessor)
{
    // GET /v1/me
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> GetMyProfile(CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            var result = (IStandardResponse)await Mediator.Send(new GetMyProfileQuery(), cancellationToken).ConfigureAwait(false);
            return result!;
        }, Logger).ConfigureAwait(false);
    }
}