namespace TradeX.Application.Abstractions.Models;

public class BaseInput<T>: ContextualRequest
{
    public T Model { get; set; }

    public BaseInput(T data)
    {
        Model = data;
    }
}
