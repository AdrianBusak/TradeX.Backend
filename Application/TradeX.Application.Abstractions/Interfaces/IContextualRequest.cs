namespace TradeX.Application.Abstractions.Interfaces;

public interface IContextualRequest
{
    Dictionary<string, object?> Context { get; }
}
