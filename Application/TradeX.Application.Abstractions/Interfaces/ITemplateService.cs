using System.Reflection;

namespace TradeX.Application.Abstractions.Interfaces;

public interface ITemplateService
{
    Task<string> ReadTextFileAsync(string path, CancellationToken cancellationToken);
    Task<string> ReadEmbeddedResourceTextFileAsync(Assembly assembly, string resourceName, CancellationToken cancellationToken);
}
