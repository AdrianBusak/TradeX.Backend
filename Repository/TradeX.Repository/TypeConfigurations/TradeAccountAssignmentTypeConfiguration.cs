using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeX.Domain.Entities;
using TradeX.Repository.Abstractions.Models;

namespace TradeX.Repository.TypeConfigurations;

public class TradeAccountAssignmentTypeConfiguration : EntityTypeConfigurationBase<TradeAccountAssignment>
{
    public override string TableName => nameof(TradeAccountAssignment);

    public override void ConfigureEntity(EntityTypeBuilder<TradeAccountAssignment> builder)
    {
        builder.Property(x => x.TradeId).IsRequired();
        builder.Property(x => x.TradingAccountId).IsRequired();
        builder.HasOne<Trade>().WithMany(x => x.AccountAssignments).HasForeignKey(x => x.TradeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<TradingAccount>().WithMany().HasForeignKey(x => x.TradingAccountId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.TradeId);
        builder.HasIndex(x => x.TradingAccountId);
        builder.HasIndex(x => new { x.TradeId, x.TradingAccountId }).IsUnique();
    }
}
