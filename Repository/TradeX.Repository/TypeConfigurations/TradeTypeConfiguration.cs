using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeX.Domain.Entities;
using TradeX.Repository.Abstractions.Models;

namespace TradeX.Repository.TypeConfigurations;

public class TradeTypeConfiguration : EntityTypeConfigurationBase<Trade>
{
    public override string TableName => nameof(Trade);

    public override void ConfigureEntity(EntityTypeBuilder<Trade> builder)
    {
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.StrategyId)
            .IsRequired();

        builder.Property(x => x.TradingInstrumentId)
            .IsRequired();

        builder.Property(x => x.Direction)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Session)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TradeDate)
            .IsRequired();

        builder.Property(x => x.EntryPrice)
            .HasPrecision(18, 6);

        builder.Property(x => x.ExitPrice)
            .HasPrecision(18, 6);

        builder.Property(x => x.StopLoss)
            .HasPrecision(18, 6);

        builder.Property(x => x.TakeProfit)
            .HasPrecision(18, 6);

        builder.Property(x => x.LotSize)
            .HasPrecision(18, 6);

        builder.Property(x => x.RiskAmount)
            .HasPrecision(18, 6);

        builder.Property(x => x.PnL)
            .HasPrecision(18, 6);

        builder.Property(x => x.RMultiple)
            .HasPrecision(18, 6);

        builder.Property(x => x.Notes)
            .HasMaxLength(4000);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Strategy>()
            .WithMany()
            .HasForeignKey(x => x.StrategyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TradingInstrument>()
            .WithMany()
            .HasForeignKey(x => x.TradingInstrumentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.StrategyId);

        builder.HasIndex(x => x.TradingInstrumentId);

        builder.HasIndex(x => x.TradeDate);
    }
}
