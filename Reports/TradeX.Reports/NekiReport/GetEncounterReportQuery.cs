using TradeX.Application.Clients.Features.Reports;

namespace TradeX.Reports.Encounter;


public class GenerateNekiReportReportQuery(Guid id, Stream? dest = null)
    : GenerateReportQuery<Guid>(id, dest)
{
    public Guid Id => Parameters;
    public override List<string> RequiredClaims => ["report.nekireport.read"];
}

public class GenerateNekiReportReportHandler(INekiReportReportGenerator generator)
    : BaseReportHandler<GenerateNekiReportReportQuery, NekiReportReportDto, Guid>(generator);



public class GenerateNekiReportReportFileNameQuery(Guid id)
    : GenerateReportFileNameQuery<Guid>(id)
{
    public Guid Id => Parameters;
    public override List<string> RequiredClaims => ["report.nekireport.read"];
}

public class GetNekiReportReportFileNameHandler(INekiReportReportGenerator reportGenerator)
    : GetReportFileNameHandler<GenerateNekiReportReportFileNameQuery, NekiReportReportDto, Guid>(reportGenerator);