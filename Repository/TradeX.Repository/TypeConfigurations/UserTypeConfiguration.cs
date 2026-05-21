using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeX.Domain.Entities;
using TradeX.Repository.Abstractions.Models;

namespace TradeX.Repository.TypeConfigurations;

public class UserTypeConfiguration : EntityTypeConfigurationBase<User>
{
    public override string TableName => nameof(User);

    public override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.ExternalId)
            .HasMaxLength(450);

        builder.HasIndex(x => x.ExternalId)
            .IsUnique();
    }
}
