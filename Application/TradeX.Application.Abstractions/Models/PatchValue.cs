namespace TradeX.Application.Abstractions.Models;

public class PatchValue<T>
{
    public T Value { get; set; }
    public bool HasChanged { get; set; } = false;

    public static implicit operator T?(PatchValue<T> patch)
    { 
        return patch.Value;
    }
}
