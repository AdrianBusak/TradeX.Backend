using Microsoft.Extensions.Configuration;
using System.Reflection;
using TradeX.Application.Abstractions.Interfaces;

namespace TradeX.Reports;

public abstract class BaseReportGenerator<TModel, TParams>(
    IConfiguration configuration): IReportGenerator<TModel, TParams>
    where TModel : class
{
    protected readonly string ConnectionString = configuration["ConnectionStrings:Db"]
                                               ?? configuration["Db"]
                                               ?? configuration.GetConnectionString("Db")!;


    protected abstract string TemplatePath { get; }

    protected abstract Task<TModel?> FetchDataAsync(TParams parameters);

    public virtual Task<string> GetFileNameAsync(TParams parameters)
        => Task.FromResult($"Report_{DateTime.Now:yyyyMMdd}.pdf");

    public async Task GenerateAsync(Stream stream, TParams parameters)
    {
        await Task.CompletedTask;
    }

    public async Task<byte[]> GenerateAsync(TParams parameters)
    {
        return await Task.FromResult<byte[]>(Array.Empty<byte>());
    }

    protected string LoadResource(string resourcePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fullResourceName = $"TradeX.Reports.{resourcePath}";

        using var stream = assembly.GetManifestResourceStream(fullResourceName)
            ?? throw new FileNotFoundException($"Resource {fullResourceName} not found.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    
}