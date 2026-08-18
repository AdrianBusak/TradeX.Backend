namespace TradeX.Application.Abstractions.Configuration;

public class ApplicationConfiguration
{
    public RequestProcessingConfiguration? RequestProcessingConfiguration { get; set; }
    public DataRetrievalConfiguration? DataRetrievalConfiguration { get; set; }
    public TradeOutcomeMlConfiguration? TradeOutcomeMlConfiguration { get; set; }
}

public class RequestProcessingConfiguration
{
    public int WarningThresholdMiliseconds { get; set; } = 500;
}

public class DataRetrievalConfiguration
{
    public int? DefaultPageSize { get; set; }
}

public class TradeOutcomeMlConfiguration
{
    public int MinimumTotalTrades { get; set; } = 100;
    public int MinimumPositiveTrades { get; set; } = 25;
    public int MinimumNonPositiveTrades { get; set; } = 25;
    public string FeatureSchemaVersion { get; set; } = "v2";
}
