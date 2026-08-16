using TradeX.Domain.Entities;
using TradeX.Repository.Abstractions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace TradeX.Repository;

public partial class TradeXDbContext(DbContextOptions<TradeXDbContext> options) : DbContext(options)
{
    public virtual DbSet<User> User { get; set; }
    public virtual DbSet<TradingAccount> TradingAccount { get; set; }
    public virtual DbSet<Strategy> Strategy { get; set; }
    public virtual DbSet<StrategyRule> StrategyRule { get; set; }
    public virtual DbSet<TradingInstrument> TradingInstrument { get; set; }
    public virtual DbSet<Trade> Trade { get; set; }
    public virtual DbSet<TradeAccountAssignment> TradeAccountAssignment { get; set; }
    public virtual DbSet<TradeImage> TradeImage { get; set; }
    public virtual DbSet<TradeRuleCheck> TradeRuleCheck { get; set; }
    public virtual DbSet<Mistake> Mistake { get; set; }
    public virtual DbSet<TradeMistake> TradeMistake { get; set; }
    public virtual DbSet<EconomicEvent> EconomicEvent { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        OnModelCreatingPartial(modelBuilder);
    }
    private static void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.RemoveOneToManyCascade();

        modelBuilder.UseCollation("Latin1_General_CI_AI");

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        modelBuilder.ApplySoftDeleteConfiguration();
        modelBuilder.ApplyBaseEntityConfiguration();

    }
}
