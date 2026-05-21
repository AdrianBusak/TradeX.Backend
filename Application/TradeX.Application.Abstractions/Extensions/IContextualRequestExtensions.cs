using TradeX.Application.Abstractions.Constants;
using TradeX.Application.Abstractions.Interfaces;

namespace TradeX.Application.Abstractions.Extensions;

public static class IContextualRequestExtensions
{
    public static Guid UserId(this IContextualRequest request)
    {
        return (Guid)request.Context[ContextKeys.UserId]!;
    }

    public static string ExternalUserId(this IContextualRequest request)
    {
        return (string)request.Context[ContextKeys.ExternalUserId]!;
    }
}
