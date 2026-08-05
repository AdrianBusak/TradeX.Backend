using API.Abstractions.Controllers;
using API.Abstractions.Extensions;
using API.Abstractions.Interfaces;
using API.Abstractions.OpenApi;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.QueryParameters;
using TradeX.Application.Clients.Features.Strategies.Commands;
using TradeX.Application.Clients.Features.Strategies.Queries;
using TradeX.Application.Clients.Features.StrategyRules.Commands;
using TradeX.Application.Clients.Features.StrategyRules.Queries;
using static TradeX.Application.Clients.Features.Strategies.Commands.CreateStrategyCommand;
using static TradeX.Application.Clients.Features.Strategies.Commands.UpdateStrategyCommand;
using static TradeX.Application.Clients.Features.StrategyRules.Commands.CreateStrategyRuleCommand;
using static TradeX.Application.Clients.Features.StrategyRules.Commands.UpdateStrategyRuleCommand;

namespace TradeX.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public class StrategiesController(
    ILogger<StrategiesController> logger,
    IMediator mediator,
    IHttpRequestProcessingService httpRequestProcessor)
    : BaseController<StrategiesController>(logger, mediator, httpRequestProcessor)
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
            var request = new GetStrategiesQuery
            {
                FilterParameters = filters?.GetQueryParameter<FilterQueryParameters>(),
                SortParameters = sort?.GetQueryParameter<SortQueryParameters>(),
                PagingParameters = paging?.GetQueryParameter<PagingQueryParameters>()
            };

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
                new GetStrategyByIdQuery(id),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Create(
        [FromBody] CreateStrategyCommandModel model,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new CreateStrategyCommand(model),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpPut("{id:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Update(
        Guid id,
        [FromBody] UpdateStrategyCommandModel model,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new UpdateStrategyCommand(id, model),
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
                new SoftDeleteStrategyCommand(id),
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
                new HardDeleteStrategyCommand(id),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpPatch("{strategyId:guid}/restore")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> Restore(Guid strategyId, CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(new RestoreStrategyCommand(strategyId), cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpGet("{strategyId:guid}/rules")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> GetRules(
        Guid strategyId,
        [FromQuery, SwaggerFilterDescription] string? filters,
        [FromQuery, SwaggerSortDescription] string? sort,
        [FromQuery, SwaggerPagingDescription] string? paging,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            var request = new GetStrategyRulesQuery(strategyId)
            {
                FilterParameters = filters?.GetQueryParameter<FilterQueryParameters>(),
                SortParameters = sort?.GetQueryParameter<SortQueryParameters>(),
                PagingParameters = paging?.GetQueryParameter<PagingQueryParameters>()
            };

            return (IStandardResponse)await Mediator.Send(request, cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpPost("{strategyId:guid}/rules")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> CreateRule(
        Guid strategyId,
        [FromBody] CreateStrategyRuleCommandModel model,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new CreateStrategyRuleCommand(strategyId, model),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpPut("{strategyId:guid}/rules/{ruleId:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> UpdateRule(
        Guid strategyId,
        Guid ruleId,
        [FromBody] UpdateStrategyRuleCommandModel model,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new UpdateStrategyRuleCommand(strategyId, ruleId, model),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpDelete("{strategyId:guid}/rules/{ruleId:guid}")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> SoftDeleteRule(
        Guid strategyId,
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new SoftDeleteStrategyRuleCommand(strategyId, ruleId),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpDelete("{strategyId:guid}/rules/{ruleId:guid}/hard")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> HardDeleteRule(
        Guid strategyId,
        Guid ruleId,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(
                new HardDeleteStrategyRuleCommand(strategyId, ruleId),
                cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }

    [HttpPatch("{strategyId:guid}/rules/{ruleId:guid}/restore")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> RestoreRule(Guid strategyId, Guid ruleId, CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            return (IStandardResponse)await Mediator.Send(new RestoreStrategyRuleCommand(strategyId, ruleId), cancellationToken).ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }
}
