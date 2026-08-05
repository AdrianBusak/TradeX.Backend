using API.Abstractions.Controllers;
using API.Abstractions.Extensions;
using API.Abstractions.Interfaces;
using API.Abstractions.OpenApi;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.QueryParameters;
using TradeX.Application.Clients.Features.TradingInstruments.Commands;
using TradeX.Application.Clients.Features.TradingInstruments.Queries;
using static TradeX.Application.Clients.Features.TradingInstruments.Commands.CreateTradingInstrumentCommand;
using static TradeX.Application.Clients.Features.TradingInstruments.Commands.UpdateTradingInstrumentCommand;

namespace TradeX.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public class TradingInstrumentsController(
    ILogger<TradingInstrumentsController> logger,
    IMediator mediator,
    IHttpRequestProcessingService processor)
    : BaseController<TradingInstrumentsController>(logger, mediator, processor)
{
    [HttpGet]
    public async Task<ContentResult> Get(
        [FromQuery, SwaggerFilterDescription] string? filters,
        [FromQuery, SwaggerSortDescription] string? sort,
        [FromQuery, SwaggerPagingDescription] string? paging,
        CancellationToken cancellationToken)
        => await ProcessAsync(
            new GetTradingInstrumentsQuery
            {
                FilterParameters = filters?.GetQueryParameter<FilterQueryParameters>(),
                SortParameters = sort?.GetQueryParameter<SortQueryParameters>(),
                PagingParameters = paging?.GetQueryParameter<PagingQueryParameters>()
            },
            cancellationToken);

    [HttpGet("lookup")]
    public async Task<ContentResult> Lookup(
        [FromQuery, SwaggerFilterDescription] string? filters,
        [FromQuery, SwaggerSortDescription] string? sort,
        [FromQuery, SwaggerPagingDescription] string? paging,
        CancellationToken cancellationToken)
        => await ProcessAsync(
            new GetTradingInstrumentsLookupQuery
            {
                FilterParameters = filters?.GetQueryParameter<FilterQueryParameters>(),
                SortParameters = sort?.GetQueryParameter<SortQueryParameters>(),
                PagingParameters = paging?.GetQueryParameter<PagingQueryParameters>()
            },
            cancellationToken);

    [HttpGet("{id:guid}")]
    public async Task<ContentResult> GetById(Guid id, CancellationToken cancellationToken)
        => await ProcessAsync(new GetTradingInstrumentByIdQuery(id), cancellationToken);

    [HttpPost]
    public async Task<ContentResult> Create(
        [FromBody] CreateTradingInstrumentCommandModel model,
        CancellationToken cancellationToken)
        => await ProcessAsync(new CreateTradingInstrumentCommand(model), cancellationToken);

    [HttpPut("{id:guid}")]
    public async Task<ContentResult> Update(
        Guid id,
        [FromBody] UpdateTradingInstrumentCommandModel model,
        CancellationToken cancellationToken)
        => await ProcessAsync(new UpdateTradingInstrumentCommand(id, model), cancellationToken);

    [HttpDelete("{id:guid}")]
    public async Task<ContentResult> SoftDelete(Guid id, CancellationToken cancellationToken)
        => await ProcessAsync(new SoftDeleteTradingInstrumentCommand(id), cancellationToken);

    [HttpDelete("{id:guid}/hard")]
    public async Task<ContentResult> HardDelete(Guid id, CancellationToken cancellationToken)
        => await ProcessAsync(new HardDeleteTradingInstrumentCommand(id), cancellationToken);

    [HttpPatch("{id:guid}/restore")]
    public async Task<ContentResult> Restore(Guid id, CancellationToken cancellationToken)
        => await ProcessAsync(new RestoreTradingInstrumentCommand(id), cancellationToken);

    private async Task<ContentResult> ProcessAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken)
        where TResponse : IStandardResponse
        => await HttpRequestProcessor.ProcessHttpRequestAsync(
            async () => await Mediator.Send(request, cancellationToken).ConfigureAwait(false),
            Logger).ConfigureAwait(false);
}
