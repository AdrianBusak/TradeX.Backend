namespace TradeX.Application.Abstractions.QueryParameters;

public class PagingQueryParameters
{
    public int Size { get; set; } = -1;
    public int Index { get; set; } = 0;
}
