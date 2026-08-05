using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeX.Domain.Entities;
using TradeX.Repository.Abstractions.Models;

namespace TradeX.Repository.TypeConfigurations;

public class TradingInstrumentTypeConfiguration : EntityTypeConfigurationBase<TradingInstrument>
{
    public override string TableName => nameof(TradingInstrument);

    public override void ConfigureEntity(EntityTypeBuilder<TradingInstrument> builder)
    {
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Symbol).HasMaxLength(30).IsRequired();
        builder.Property(x => x.MarketType).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.Symbol }).IsUnique();
    }
}
