using TradeX.Application.Abstractions.Interfaces;

namespace TradeX.Application.Abstractions.Models;

public class ContextualRequest : IContextualRequest
{
    private readonly Dictionary<string, object?> _context = new();

    public Dictionary<string, object?> Context
    { 
        get
        {
            return _context;
        }
    }
}
