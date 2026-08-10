using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeX.Domain.Entities;
using TradeX.Repository.Abstractions.Models;

namespace TradeX.Repository.TypeConfigurations;

public class TradeRuleCheckTypeConfiguration : EntityTypeConfigurationBase<TradeRuleCheck>
{
    public override string TableName => "TradeRuleChecks";

    public override void ConfigureEntity(EntityTypeBuilder<TradeRuleCheck> builder)
    {
        builder.Property(x => x.TradeId).IsRequired();
        builder.Property(x => x.StrategyRuleId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.IsFollowed).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(1000);

        builder.HasOne<Trade>()
            .WithMany(x => x.RuleChecks)
            .HasForeignKey(x => x.TradeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<StrategyRule>()
            .WithMany(x => x.TradeRuleChecks)
            .HasForeignKey(x => x.StrategyRuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TradeId, x.StrategyRuleId }).IsUnique();
        builder.HasIndex(x => x.TradeId);
        builder.HasIndex(x => x.StrategyRuleId);
        builder.HasIndex(x => x.IsFollowed);
        builder.HasIndex(x => x.UserId);
    }
}
