using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeX.Domain.Entities;
using TradeX.Repository.Abstractions.Models;

namespace TradeX.Repository.TypeConfigurations;

public class StrategyRuleTypeConfiguration : EntityTypeConfigurationBase<StrategyRule>
{
    public override string TableName => nameof(StrategyRule);

    public override void ConfigureEntity(EntityTypeBuilder<StrategyRule> builder)
    {
        builder.Property(x => x.StrategyId)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Order)
            .IsRequired();

        builder.Property(x => x.IsRequired)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Importance)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne<Strategy>()
            .WithMany()
            .HasForeignKey(x => x.StrategyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.StrategyId);
    }
}
