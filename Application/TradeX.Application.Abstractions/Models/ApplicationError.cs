using TradeX.Application.Abstractions.Interfaces;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace TradeX.Application.Abstractions.Models;

public class ApplicationError(object data) : IError
{
    public object? Data { get; set; } = data;

    private static JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

    public virtual string GetMessage()
    {
        string msg;

        if (Data is Exception ex)
        {
            msg = ex.Message;
        }
        else if (Data is string strData)
        {
            msg = strData;
        }
        else
        {            
            msg = $"[{JsonSerializer.Serialize(Data, JsonSerializerOptions)}]";
        }

        return msg;
    }

    public virtual string GetUserFriendlyMessage()
    {
        if (Debugger.IsAttached)
        {
            return GetMessage();
        }

        return "Something went wrong!";
    }
}
