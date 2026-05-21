using TradeX.Application.Abstractions.Enums;

namespace TradeX.Application.Abstractions.Interfaces;

public interface IStandardResponse
{
    OperationResult Result { get; set; }
    string? Message { get; set; }
    object? Error { get; set; }
}

