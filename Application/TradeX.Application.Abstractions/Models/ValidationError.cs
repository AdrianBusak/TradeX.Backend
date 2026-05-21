using System.Text.Encodings.Web;
using System.Text.Json;

namespace TradeX.Application.Abstractions.Models;

public class ValidationError : ApplicationError
{
    public ValidationError(string operationName, Dictionary<string, string[]> failureMessages) : base(failureMessages)
    {
        OperationName = operationName;
    }

    public string OperationName { get; set; }

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    public override string GetMessage()
    {
        string msg = $"Data validation error: [OperationName: {OperationName}] [{JsonSerializer.Serialize(Data, _jsonSerializerOptions)}]";

        return msg;
    }

    public override string GetUserFriendlyMessage()
    {
        return GetMessage();
    }
}
