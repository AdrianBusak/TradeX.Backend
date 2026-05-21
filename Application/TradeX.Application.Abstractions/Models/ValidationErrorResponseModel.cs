namespace TradeX.Application.Abstractions.Models;

public class ValidationErrorResponseModel
{
    public Dictionary<string, string[]> ValidationErrors { get; set; } = new Dictionary<string, string[]>();
}
