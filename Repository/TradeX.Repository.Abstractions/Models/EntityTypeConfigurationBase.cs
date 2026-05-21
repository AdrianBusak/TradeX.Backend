using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeX.Domain.Abstractions.Interfaces;

namespace TradeX.Repository.Abstractions.Models;

public abstract class EntityTypeConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class, IBaseEntity
{
    public abstract string TableName { get; }

    public virtual void ConfigureEntity(EntityTypeBuilder<TEntity> builder)
    {
    }

    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        ConfigureEntity(builder);
        builder.ToTable(TableName);
        builder.HasKey((TEntity e) => e.Id);
        builder.Property((TEntity x) => x.Id).ValueGeneratedOnAdd();
    }
}
