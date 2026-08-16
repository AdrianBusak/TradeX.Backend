using System.Text.Json.Serialization;

namespace TradeX.Infrastructure.EconomicCalendar.Services;

public sealed class ForexFactoryEconomicEventDto
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("impact")]
    public string? Impact { get; set; }

    [JsonPropertyName("forecast")]
    public string? Forecast { get; set; }

    [JsonPropertyName("previous")]
    public string? Previous { get; set; }
}
