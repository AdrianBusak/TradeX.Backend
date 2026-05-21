using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MediatR;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Extensions;
using TradeX.Application.Abstractions.Configuration;

namespace TradeX.Application.Abstractions.Behaviors;

public class PerformanceBehavior<TRequest, TResponse>(ILogger<TRequest> logger, ApplicationConfiguration config) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger = logger;
    private readonly ApplicationConfiguration _config = config;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_config == null)
        {
            throw new NullReferenceException("ApplicationConfiguration cannot be null. Check Dependency Injection.");
        }

        if (_logger == null)
        {
            throw new NullReferenceException("ILogger<TRequest> cannot be null. Check Dependency Injection.");
        }

        TResponse response;

        var timer = new Stopwatch();

        try
        {
            timer.Start();

            response = await next();
        }
        finally
        {
            timer.Stop();
            try
            {
                if (timer.ElapsedMilliseconds > _config!.RequestProcessingConfiguration!.WarningThresholdMiliseconds)
                {
                    var name = typeof(TRequest).Name;
                    string msg = $"Long Running Request: [RequestName: {name}] [Elapsed Miliseconds: {timer.ElapsedMilliseconds}]";

                    Guid userId = Guid.Empty;
                    if (request is IAuthenticatedRequest authRequest)
                    {
                        try
                        {
                            userId = authRequest.UserId();
                        }
                        catch (Exception)
                        {
                        }
                    }

                    _logger.LogWarning("{Message} [UserId: {userId}]", msg, userId);
                }
            }
            catch (Exception)
            {

            }
        }

        return response;
    }
}
