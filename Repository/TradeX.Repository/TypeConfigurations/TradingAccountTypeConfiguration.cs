using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeX.Domain.Entities;
using TradeX.Repository.Abstractions.Models;

namespace TradeX.Repository.TypeConfigurations;

public class TradingAccountTypeConfiguration : EntityTypeConfigurationBase<TradingAccount>
{
    public override string TableName => nameof(TradingAccount);

    public override void ConfigureEntity(EntityTypeBuilder<TradingAccount> builder)
    {
        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.AccountType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Broker)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsFixedLength()
            .IsRequired();

        builder.Property(x => x.InitialBalance)
            .HasPrecision(18, 2);

        builder.Property(x => x.CurrentBalance)
            .HasPrecision(18, 2);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => new { x.UserId, x.Name })
            .IsUnique();
    }
}
