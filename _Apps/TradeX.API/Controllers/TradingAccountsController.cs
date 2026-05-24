using API.Abstractions.Controllers;
using API.Abstractions.Extensions;
using API.Abstractions.Interfaces;
using API.Abstractions.OpenApi;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.QueryParameters;
using TradeX.Application.Clients.Features.TradingAccounts.Commands;
using TradeX.Application.Clients.Features.TradingAccounts.Queries;
using static TradeX.Application.Clients.Features.TradingAccounts.Commands.CreateTradingAccountCommand;
using static TradeX.Application.Clients.Features.TradingAccounts.Commands.UpdateTradingAccountCommand;

namespace TradeX.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public class TradingAccountsController(
    ILogger<TradingAccountsController> logger,
    IMediator mediator,
    IHttpRequestProcessingService httpRequestProcessor)
    : BaseController<TradingAccountsController>(logger, mediator, httpRequestProcessor)
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
            var request = new GetTradingAccountsQuery
            {
                FilterParameters = filters?.GetQueryParameter<FilterQueryParameters>(),
                SortParameters = sort?.GetQueryParameter<SortQueryParameters>(),
                PagingParameters = paging?.GetQueryParameter<PagingQueryParameters>()
            };

            return (IStandardResponse)await Mediator.Send(request, cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpGet("lookup")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> GetLookup(
        [FromQuery, SwaggerFilterDescription] string? filters,
        [FromQuery, SwaggerSortDescription] string? sort,
        [FromQuery, SwaggerPagingDescription] string? paging,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            var request = new GetTradingAccountsLookupQuery(
                filters?.GetQueryParameter<FilterQueryParameters>(),
                paging?.GetQueryParameter<PagingQueryParameters>(),
                sort?.GetQueryParameter<SortQueryParameters>());

            return (IStandardResponse)await Mediator.Send(request, cancellationToken).ConfigureAwait(false);
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
                new GetTradingAccountByIdQuery(id),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Create(
        [FromBody] CreateTradingAccountCommandModel model,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new CreateTradingAccountCommand(model),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Update(
        Guid id,
        [FromBody] UpdateTradingAccountCommandModel model,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new UpdateTradingAccountCommand(id, model),
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
                new SoftDeleteTradingAccountCommand(id),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpDelete("{id:guid}/hard")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> HardDelete(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new HardDeleteTradingAccountCommand(id),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpPatch("{id:guid}/restore")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Restore(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new RestoreTradingAccountCommand(id),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }
}
