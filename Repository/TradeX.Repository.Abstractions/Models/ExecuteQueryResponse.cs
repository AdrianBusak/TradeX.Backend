namespace TradeX.Repository.Abstractions.Models
{
    public class ExecuteQueryResponse<TResponse>
    {
        public long TotalRecordCount { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public List<TResponse>? Records { get; set; }
    }
}
