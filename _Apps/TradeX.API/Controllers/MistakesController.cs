using API.Abstractions.Controllers;
using API.Abstractions.Interfaces;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Clients.Features.Mistakes.Commands;
using TradeX.Application.Clients.Features.Mistakes.Queries;
using static TradeX.Application.Clients.Features.Mistakes.Commands.CreateMistakeCommand;
using static TradeX.Application.Clients.Features.Mistakes.Commands.UpdateMistakeCommand;

namespace TradeX.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public class MistakesController(
    ILogger<MistakesController> logger,
    IMediator mediator,
    IHttpRequestProcessingService httpRequestProcessor)
    : BaseController<MistakesController>(logger, mediator, httpRequestProcessor)
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Get(CancellationToken cancellationToken)
        => await ProcessAsync(new GetMistakesQuery(), cancellationToken).ConfigureAwait(false);

    [HttpGet("{mistakeId:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> GetById(
        Guid mistakeId,
        CancellationToken cancellationToken)
        => await ProcessAsync(new GetMistakeByIdQuery(mistakeId), cancellationToken)
            .ConfigureAwait(false);

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Create(
        [FromBody] CreateMistakeRequest model,
        CancellationToken cancellationToken)
        => await ProcessAsync(new CreateMistakeCommand(model), cancellationToken).ConfigureAwait(false);

    [HttpPut("{mistakeId:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Update(
        Guid mistakeId,
        [FromBody] UpdateMistakeRequest model,
        CancellationToken cancellationToken)
        => await ProcessAsync(new UpdateMistakeCommand(mistakeId, model), cancellationToken).ConfigureAwait(false);

    [HttpDelete("{mistakeId:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Delete(Guid mistakeId, CancellationToken cancellationToken)
        => await ProcessAsync(new SoftDeleteMistakeCommand(mistakeId), cancellationToken).ConfigureAwait(false);

    [HttpPatch("{mistakeId:guid}/restore")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Restore(Guid mistakeId, CancellationToken cancellationToken)
        => await ProcessAsync(new RestoreMistakeCommand(mistakeId), cancellationToken).ConfigureAwait(false);

    [HttpDelete("{mistakeId:guid}/hard")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> HardDelete(Guid mistakeId, CancellationToken cancellationToken)
        => await ProcessAsync(new HardDeleteMistakeCommand(mistakeId), cancellationToken).ConfigureAwait(false);

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
