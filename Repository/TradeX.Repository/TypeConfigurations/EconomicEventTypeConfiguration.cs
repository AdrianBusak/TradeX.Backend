using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeX.Domain.Entities;
using TradeX.Repository.Abstractions.Models;

namespace TradeX.Repository.TypeConfigurations;

public sealed class EconomicEventTypeConfiguration : EntityTypeConfigurationBase<EconomicEvent>
{
    public override string TableName => "EconomicEvents";

    public override void ConfigureEntity(EntityTypeBuilder<EconomicEvent> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        builder.Property(x => x.ScheduledAt).IsRequired();
        builder.Property(x => x.Impact).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Forecast).HasMaxLength(100);
        builder.Property(x => x.Previous).HasMaxLength(100);
        builder.Property(x => x.LastSyncedAt).IsRequired();

        builder.HasIndex(x => new { x.Title, x.Currency, x.ScheduledAt }).IsUnique();
        builder.HasIndex(x => x.ScheduledAt);
    }
}
