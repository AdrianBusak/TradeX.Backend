using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeX.Domain.Entities;
using TradeX.Repository.Abstractions.Models;

namespace TradeX.Repository.TypeConfigurations;

public class MistakeTypeConfiguration : EntityTypeConfigurationBase<Mistake>
{
    public override string TableName => "Mistakes";

    public override void ConfigureEntity(EntityTypeBuilder<Mistake> builder)
    {
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);

        builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.IsActive });
    }
}
