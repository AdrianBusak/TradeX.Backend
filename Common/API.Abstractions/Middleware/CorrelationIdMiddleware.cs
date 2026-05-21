namespace API.Abstractions.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
        Guid correlationId;

        if (context.Request.Headers.TryGetValue("X-Correlation-Id", out var cidHeader)
            && Guid.TryParse(cidHeader, out var parsed))
        {
            correlationId = parsed;
        }
        else
        {
            correlationId = Guid.NewGuid();
        }

        CorrelationContext.CorrelationId = correlationId;

        context.Response.Headers["X-Correlation-Id"] = correlationId.ToString();

        await _next(context);
    }
}

public static class CorrelationContext
{
    private static readonly AsyncLocal<Guid?> _correlationId = new();

    public static Guid CorrelationId
    {
        get
        {
            if (_correlationId.Value == null)
                _correlationId.Value = Guid.NewGuid();

            return _correlationId.Value.Value;
        }
        set => _correlationId.Value = value;
    }
}