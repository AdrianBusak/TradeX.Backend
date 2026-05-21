using TradeX.Application.Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Abstractions.Interfaces;

public interface IHttpRequestProcessingService
{
    Task<ContentResult> ProcessHttpRequestAsync(Func<Task<IStandardResponse>> operation, ILogger log);
}
