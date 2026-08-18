using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeX.Domain.Entities;
using TradeX.Repository.Abstractions.Models;

namespace TradeX.Repository.TypeConfigurations;

public class UserTradeOutcomeModelTypeConfiguration
    : EntityTypeConfigurationBase<UserTradeOutcomeModel>
{
    public override string TableName => "UserTradeOutcomeModels";

    public override void ConfigureEntity(EntityTypeBuilder<UserTradeOutcomeModel> builder)
    {
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.ModelVersion).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ModelPath).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.SampleCount).IsRequired();
        builder.Property(x => x.PositiveCount).IsRequired();
        builder.Property(x => x.NonPositiveCount).IsRequired();
        builder.Property(x => x.TrainedAt).IsRequired();
        builder.Property(x => x.FeatureSchemaVersion).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsActiveModel).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.IsActiveModel });
        builder.HasIndex(x => new { x.UserId, x.ModelVersion });
    }
}
