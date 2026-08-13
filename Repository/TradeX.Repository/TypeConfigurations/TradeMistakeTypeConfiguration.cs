using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeX.Domain.Entities;
using TradeX.Repository.Abstractions.Models;

namespace TradeX.Repository.TypeConfigurations;

public class TradeMistakeTypeConfiguration : EntityTypeConfigurationBase<TradeMistake>
{
    public override string TableName => "TradeMistakes";

    public override void ConfigureEntity(EntityTypeBuilder<TradeMistake> builder)
    {
        builder.Property(x => x.TradeId).IsRequired();
        builder.Property(x => x.MistakeId).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(1000);

        builder.HasOne<Trade>().WithMany(x => x.Mistakes).HasForeignKey(x => x.TradeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Mistake>().WithMany(x => x.TradeMistakes).HasForeignKey(x => x.MistakeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.TradeId, x.MistakeId }).IsUnique();
        builder.HasIndex(x => x.TradeId);
        builder.HasIndex(x => x.MistakeId);
    }
}
