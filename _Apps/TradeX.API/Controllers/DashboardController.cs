using API.Abstractions.Controllers;
using API.Abstractions.Interfaces;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Clients.Features.Dashboard.Queries;

namespace TradeX.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/dashboard")]
public class DashboardController(
    ILogger<DashboardController> logger,
    IMediator mediator,
    IHttpRequestProcessingService httpRequestProcessor)
    : BaseController<DashboardController>(logger, mediator, httpRequestProcessor)
{
    [HttpGet("trading-summary")]
    [MapToApiVersion("1.0")]
    public async Task<ContentResult> GetTradingSummary(
        [FromQuery] TradingDashboardPeriod? period,
        [FromQuery] DateOnly? dateFrom,
        [FromQuery] DateOnly? dateTo,
        [FromQuery] Guid? strategyId,
        [FromQuery] Guid? accountId,
        CancellationToken cancellationToken)
    {
        return await HttpRequestProcessor.ProcessHttpRequestAsync(async () =>
        {
            var request = new GetTradingDashboardSummaryQuery
            {
                Period = period,
                DateFrom = dateFrom,
                DateTo = dateTo,
                StrategyId = strategyId,
                AccountId = accountId
            };

            return (IStandardResponse)await Mediator.Send(request, cancellationToken)
                .ConfigureAwait(false);
        }, Logger).ConfigureAwait(false);
    }
}
