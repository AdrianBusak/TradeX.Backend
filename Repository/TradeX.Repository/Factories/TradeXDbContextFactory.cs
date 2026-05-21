using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TradeX.Repository;

namespace TradeX.Repository.Factories;

public class TradeXDbContextFactory : IDesignTimeDbContextFactory<TradeXDbContext>
{
    public TradeXDbContext CreateDbContext(string[] args)
    {
        string? connectionString = args.Length != 0 ? args[0] : null;

        var dbOptions = GenerateDbOptions(connectionString);

        return new TradeXDbContext(dbOptions);
    }

    private static DbContextOptions<TradeXDbContext> GenerateDbOptions(string? connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TradeXDbContext>();

        if (string.IsNullOrEmpty(connectionString))
        {
            optionsBuilder.UseSqlServer(o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery));
        }
        else
        {
            optionsBuilder.UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery));
        }

        return optionsBuilder.Options;
    }
}
