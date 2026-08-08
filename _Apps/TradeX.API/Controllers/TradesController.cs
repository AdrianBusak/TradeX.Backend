using API.Abstractions.Controllers;
using API.Abstractions.Extensions;
using API.Abstractions.Interfaces;
using API.Abstractions.OpenApi;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.QueryParameters;
using TradeX.Application.Clients.Features.Trades.Commands;
using TradeX.Application.Clients.Features.Trades.Queries;
using static TradeX.Application.Clients.Features.Trades.Commands.CreateTradeCommand;
using static TradeX.Application.Clients.Features.Trades.Commands.UpdateTradeCommand;

namespace TradeX.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public class TradesController(
    ILogger<TradesController> logger,
    IMediator mediator,
    IHttpRequestProcessingService httpRequestProcessor)
    : BaseController<TradesController>(logger, mediator, httpRequestProcessor)
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Get(
        [FromQuery, SwaggerFilterDescription] string? filters,
        [FromQuery, SwaggerSortDescription] string? sort,
        [FromQuery, SwaggerPagingDescription] string? paging,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            var request = new GetTradesQuery
            {
                FilterParameters = filters?.GetQueryParameter<FilterQueryParameters>(),
                SortParameters = sort?.GetQueryParameter<SortQueryParameters>(),
                PagingParameters = paging?.GetQueryParameter<PagingQueryParameters>()
            };

            return (IStandardResponse)await Mediator.Send(request, cancellationToken)
                .ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new GetTradeByIdQuery(id),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Create(
        [FromBody] CreateTradeCommandModel model,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new CreateTradeCommand(model),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpPost("{tradeId:guid}/images")]
    [MapToApiVersion("1.0")]
    [Consumes("multipart/form-data")]
    public async Task<ContentResult> UploadImage(
        Guid tradeId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            await using var stream = file.OpenReadStream();
            return (IStandardResponse)await Mediator.Send(
                new UploadTradeImageCommand(tradeId, stream, file.FileName, file.Length),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpGet("{tradeId:guid}/images")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> GetImages(Guid tradeId, CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new GetTradeImagesQuery(tradeId), cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpDelete("{tradeId:guid}/images/{imageId:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> DeleteImage(Guid tradeId, Guid imageId, CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new DeleteTradeImageCommand(tradeId, imageId), cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Update(
        Guid id,
        [FromBody] UpdateTradeCommandModel model,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new UpdateTradeCommand(id, model),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpDelete("{id:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> SoftDelete(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new SoftDeleteTradeCommand(id),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpDelete("{id:guid}/hard")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> HardDelete(Guid id, CancellationToken cancellationToken)
    {

        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(new HardDeleteTradeCommand(id), cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }
    
    [HttpPatch("{id:guid}/restore")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(new RestoreTradeCommand(id), cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

}
