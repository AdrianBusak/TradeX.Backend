namespace TradeX.Application.Abstractions.Configuration;

public class ApplicationConfiguration
{
    public RequestProcessingConfiguration? RequestProcessingConfiguration { get; set; }
    public DataRetrievalConfiguration? DataRetrievalConfiguration { get; set; }
}

public class RequestProcessingConfiguration
{
    public int WarningThresholdMiliseconds { get; set; } = 500;
}

public class DataRetrievalConfiguration
{
    public int? DefaultPageSize { get; set; }
}
