namespace TradeX.Application.Abstractions.Interfaces;

public interface IReportGenerator<TModel, TParams>
    where TModel : class
{
    Task<string> GetFileNameAsync(TParams parameters);
    Task GenerateAsync(Stream stream, TParams parameters);
    Task<byte[]> GenerateAsync(TParams parameters);
}
