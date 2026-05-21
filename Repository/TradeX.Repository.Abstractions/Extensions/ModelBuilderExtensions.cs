using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TradeX.Domain.Abstractions.Interfaces;

namespace TradeX.Repository.Abstractions.Extensions;

public static class ModelBuilderExtensions
{
    public static void RemoveOneToManyCascade(this ModelBuilder builder)
    {
        builder.EntityLoop(delegate (IMutableEntityType et)
        {
            (from fk in et.GetForeignKeys()
             where fk.DeleteBehavior == DeleteBehavior.Cascade
             select fk).ToList().ForEach(delegate (IMutableForeignKey fk)
             {
                 fk.DeleteBehavior = DeleteBehavior.Restrict;
             });
        });
    }

    private static void EntityLoop(this ModelBuilder builder, Action<IMutableEntityType> action)
    {
        foreach (IMutableEntityType entityType in builder.Model.GetEntityTypes())
        {
            action(entityType);
        }
    }

    public static void ApplySoftDeleteConfiguration(this ModelBuilder modelBuilder)
    {
        // Get all entity types that implement IAuditableEntityWithSoftDelete
        var softDeleteEntities = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(IAuditableEntityWithSoftDelete).IsAssignableFrom(e.ClrType));

        foreach (var entityType in softDeleteEntities)
        {
            // 1. Set default value for the column in the database schema
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(IAuditableEntityWithSoftDelete.IsActive))
                .HasDefaultValue(true);
        }
    }

    public static void ApplyBaseEntityConfiguration(this ModelBuilder modelBuilder)
    {
        // Get all entity types that implement IBaseEntity
        var baseEntities = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(IBaseEntity).IsAssignableFrom(e.ClrType));

        foreach (var entityType in baseEntities)
        {
            // Configure CreatedAt to have a database-side default value
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(IBaseEntity.CreatedAt))
                .HasDefaultValueSql("GETUTCDATE()") // Use UTC for consistency
                .ValueGeneratedOnAdd(); // Ensures EF knows the DB generates this

            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(IBaseEntity.ModifiedAt))
                .HasDefaultValueSql("GETUTCDATE()") // Use UTC for consistency
                .ValueGeneratedOnAdd(); // Ensures EF knows the DB generates this
        }
    }

    private static System.Linq.Expressions.LambdaExpression ConvertFilterExpression(Type type)
    {
        // Generates: e => e.IsDeleted == false
        var parameter = System.Linq.Expressions.Expression.Parameter(type, "e");
        var property = System.Linq.Expressions.Expression.Property(parameter, nameof(IAuditableEntityWithSoftDelete.IsActive));
        var falseConstant = System.Linq.Expressions.Expression.Constant(false);
        var comparison = System.Linq.Expressions.Expression.Equal(property, falseConstant);

        return System.Linq.Expressions.Expression.Lambda(comparison, parameter);
    }
}
