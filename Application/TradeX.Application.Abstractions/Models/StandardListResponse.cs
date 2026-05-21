using TradeX.Application.Abstractions.Enums;

namespace TradeX.Application.Abstractions.Models;

public class StandardListResponse<TResponseModel> : StandardResponse<List<TResponseModel>>
{
    public long TotalRecordCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }

    public StandardListResponse(List<TResponseModel> model) : base(model)
    {
        Result = OperationResult.Ok;
    }
    
    public StandardListResponse(List<TResponseModel> model, long totalRecordCount, int pageIndex, int pageSize) : this(model)
    {
        TotalRecordCount = totalRecordCount;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }

    public StandardListResponse(OperationResult result, string message, object error) : base(result, message, error)
    {
    }
}
