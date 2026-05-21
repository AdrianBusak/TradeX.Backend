using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TradeX.Application.Abstractions.Interfaces;

namespace TradeX.Reports.Encounter;

public interface IEncounterReportGenerator: IReportGenerator<EncounterReportDto, Guid>;

public class EncounterReportGenerator(IConfiguration configuration)
    : BaseReportGenerator<EncounterReportDto, Guid>(configuration), IEncounterReportGenerator
{
    protected override string TemplatePath => "Encounter.EncounterReport.scriban.html";

    public override async Task<string> GetFileNameAsync(Guid encounterId)
    {
        using var connection = new SqlConnection(ConnectionString);
        
        var key = await connection.ExecuteScalarAsync<string>(
            "SELECT [Key] FROM [Encounter] WHERE Id = @EncounterId",
            new { encounterId });

        return $"Encounter_{key ?? encounterId.ToString()}.pdf";
    }

    protected override async Task<EncounterReportDto?> FetchDataAsync(Guid encounterId)
    {
        using var connection = new SqlConnection(ConnectionString);
        var sql = LoadResource("Encounter.EncounterReport.sql");

        using var multi = await connection.QueryMultipleAsync(sql, new { encounterId });

        var report = await multi.ReadFirstOrDefaultAsync<EncounterReportDto>();

        if (report != null)
        {
            report.Injuries = (await multi.ReadAsync<EncounterInjuryDto>()).ToList();
        }

        return report;
    }
}