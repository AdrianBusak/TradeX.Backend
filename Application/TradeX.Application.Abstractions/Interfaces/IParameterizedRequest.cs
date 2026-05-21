using TradeX.Application.Abstractions.QueryParameters;

namespace TradeX.Application.Abstractions.Interfaces;

public interface IParameterizedRequest
{
    public PagingQueryParameters? PagingParameters { get; set; }
    public FilterQueryParameters? FilterParameters { get; set; }
    public SortQueryParameters? SortParameters { get; set; }
}
