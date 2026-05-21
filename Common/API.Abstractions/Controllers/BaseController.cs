using API.Abstractions.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Abstractions.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public abstract class BaseController<TController>(
    ILogger<TController> logger,
    IMediator mediator,
    IHttpRequestProcessingService httpRequestProcessor) : ControllerBase
{
    protected readonly ILogger<TController> Logger = logger;
    protected readonly IMediator Mediator = mediator;
    protected readonly IHttpRequestProcessingService HttpRequestProcessor = httpRequestProcessor;
}
