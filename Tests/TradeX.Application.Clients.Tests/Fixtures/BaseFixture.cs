using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TradeX.Application.Clients.Tests.Extensions;

namespace TradeX.Application.Clients.Tests.Fixtures;

public class BaseFixture
{
    protected readonly IServiceProvider _serviceProvider;
    protected readonly IMediator _mediator;

    public BaseFixture()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTestServices();

        _serviceProvider = serviceCollection.BuildServiceProvider();

        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    protected static string ToBase64StringAsync(MemoryStream stream)
    {
        if (stream == null || !stream.CanRead)
            throw new ArgumentException("Invalid stream provided.");

        byte[] bytes = stream.ToArray();
        return Convert.ToBase64String(bytes);
    }

    protected static MemoryStream GetMemoryStreamFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var memoryStream = new MemoryStream();

        fileStream.CopyTo(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }
}
