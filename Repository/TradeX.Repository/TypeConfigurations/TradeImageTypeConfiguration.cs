using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeX.Domain.Entities;
using TradeX.Repository.Abstractions.Models;

namespace TradeX.Repository.TypeConfigurations;

public class TradeImageTypeConfiguration : EntityTypeConfigurationBase<TradeImage>
{
    public override string TableName => nameof(TradeImage);

    public override void ConfigureEntity(EntityTypeBuilder<TradeImage> builder)
    {
        builder.Property(x => x.TradeId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.BlobPath).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SizeBytes).IsRequired();
        builder.HasOne<Trade>().WithMany(x => x.Images).HasForeignKey(x => x.TradeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.TradeId);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.BlobPath).IsUnique();
    }
}
