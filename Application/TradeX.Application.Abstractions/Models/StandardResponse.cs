using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Interfaces;

namespace TradeX.Application.Abstractions.Models;

public class StandardResponse<T> : IStandardResponse
{
    public OperationResult Result { get; set; }
    public string? Message { get; set; }
    public object? Error { get; set; }

    public StandardResponse(OperationResult result, T model)
    {
        Result = result;
        Model = model;
    }
    public StandardResponse(T model): this(OperationResult.Ok, model) { }
    
    public StandardResponse(OperationResult result, string message, object error)
    {
        Result = result;
        Message = message;
        SetErrorObject(error);
    }

    public T? Model { get; set; }
 
    protected virtual void SetErrorObject(object error)
    {
        Error = error;
    }
}