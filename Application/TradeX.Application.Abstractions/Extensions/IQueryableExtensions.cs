using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.QueryParameters;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace TradeX.Application.Abstractions.Extensions;

public static class IQueryableExtensions
{
    public static IQueryable<T> OrderBySortParameters<T>(this IQueryable<T> query, SortQueryParameters? sortParameters)
    {
        if (sortParameters == null)
        {
            return query;
        }

        for (int i = 0; i < sortParameters.Count; i++)
        {
            SortQueryParameter sortParameter = sortParameters[i];
            string text = sortParameter.FieldName.FirstCharToUpperInvariant();
            
            //_ = typeof(T).FullName + "." + text;
            
            Type? propertyType = GetPropertyTypeCached<T>(text);

            if (propertyType != null)
            {
                string key = $"{typeof(T).FullName}.{"GetPropertySelectorExpression"}.{propertyType.FullName}";
                object obj = GenericMethodCache.GetOrAdd(key, ctx => typeof(IQueryableExtensions).GetMethod("GetPropertySelectorExpression")!.MakeGenericMethod(typeof(T), propertyType)).Invoke(null, [text])!;
                string key2 = $"{typeof(T).FullName}.{"OrderByDirection"}.{propertyType.FullName}";
                query = (IQueryable<T>)GenericMethodCache.GetOrAdd(key2, ctx => typeof(IQueryableExtensions).GetMethod("OrderByDirection")!.MakeGenericMethod(typeof(T), propertyType)).Invoke(null, [query, obj, sortParameter.Direction])!;
            }
        }

        return query;
    }

    private static readonly ConcurrentDictionary<string, Type?> PropertyCache = new();
    private static readonly ConcurrentDictionary<string, MethodInfo> GenericMethodCache = new();
    private static readonly ConcurrentDictionary<string, object> PropertySelectorExpressionCache = new();

    public static Expression<Func<T, TProperty>> GetPropertySelectorExpression<T, TProperty>(this string propertyName)
    {
        string propertyName2 = propertyName;
        string key = typeof(T).FullName + "." + propertyName2;
        return (Expression<Func<T, TProperty>>)PropertySelectorExpressionCache.GetOrAdd(key, delegate
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T));
            return Expression.Lambda<Func<T, TProperty>>(Expression.Property(parameterExpression, typeof(T), propertyName2), [parameterExpression]);
        });
    }
    private static Type? GetPropertyTypeCached<T>(string propertyName)
    {
        string propertyName2 = propertyName;
        string key = typeof(T).FullName + "." + propertyName2;
        return PropertyCache.GetOrAdd(key, x => typeof(T).GetProperty(propertyName2)?.PropertyType);
    }

    public static IQueryable<T> OrderByDirection<T, TProperty>(this IQueryable<T> query, Expression<Func<T, TProperty>> propSelector, SortDirection sortDirection)
    {
        if (query.IsOrdered())
        {
            IOrderedQueryable<T> source = (query as IOrderedQueryable<T>)!;
            query = sortDirection == SortDirection.Asc ? source.ThenBy(propSelector) : source.ThenByDescending(propSelector);
        }
        else
        {
            query = sortDirection == SortDirection.Asc ? query.OrderBy(propSelector) : query.OrderByDescending(propSelector);
        }

        return query;
    }

    public static bool IsOrdered<T>(this IQueryable<T> query)
    {
        ArgumentNullException.ThrowIfNull(query);

        return query.Expression.Type == typeof(IOrderedQueryable<T>);
    }

    public static IQueryable<T> ApplyDateFilter<T>(this IQueryable<T> query,
        FilterQueryParameterDeconstructed<DateTime?>? filter,
        Expression<Func<T, DateTime?>> selector)
    {
        if (filter == null)
            return query;

        if (filter.Eq != null)
            query = query.Where(BuildBinary(selector, ExpressionType.Equal, filter.Eq));

        if (filter.Neq != null)
            query = query.Where(BuildBinary(selector, ExpressionType.NotEqual, filter.Neq));

        if (filter.Gt != null)
            query = query.Where(BuildBinary(selector, ExpressionType.GreaterThan, filter.Gt));

        if (filter.Gte != null)
            query = query.Where(BuildBinary(selector, ExpressionType.GreaterThanOrEqual, filter.Gte));

        if (filter.Lt != null)
            query = query.Where(BuildBinary(selector, ExpressionType.LessThan, filter.Lt));

        if (filter.Lte != null)
            query = query.Where(BuildBinary(selector, ExpressionType.LessThanOrEqual, filter.Lte));

        return query;
    }

    public static IQueryable<T> ApplyStringFilter<T>(this IQueryable<T> query,
        FilterQueryParameterDeconstructed<string?>? filter,
        Expression<Func<T, string?>> selector)
    {
        if (filter == null)
            return query;

        var parameter = selector.Parameters[0];
        var member = selector.Body;

        if (!string.IsNullOrWhiteSpace(filter.Eq))
        {
            var constant = Expression.Constant(filter.Eq);
            var body = Expression.Equal(member, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
            query = query.Where(lambda);
        }

        if (!string.IsNullOrWhiteSpace(filter.Neq))
        {
            var constant = Expression.Constant(filter.Neq);
            var body = Expression.NotEqual(member, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
            query = query.Where(lambda);
        }

        if (!string.IsNullOrWhiteSpace(filter.Contains))
        {
            var method = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
            var constant = Expression.Constant(filter.Contains);
            var body = Expression.Call(member, method, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
            query = query.Where(lambda);
        }

        if (!string.IsNullOrWhiteSpace(filter.StartsWith))
        {
            var method = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;
            var constant = Expression.Constant(filter.StartsWith);
            var body = Expression.Call(member, method, constant);
            var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);
            query = query.Where(lambda);
        }

        return query;
    }


    public static IQueryable<T> ApplyLongFilter<T>(
    this IQueryable<T> query,
    FilterQueryParameterDeconstructed<long?>? filter,
    Expression<Func<T, long?>> selector)
    {
        if (filter == null)
            return query;

        if (filter.Eq != null)
            query = query.Where(BuildBinary(selector, ExpressionType.Equal, filter.Eq));

        if (filter.Neq != null)
            query = query.Where(BuildBinary(selector, ExpressionType.NotEqual, filter.Neq));

        if (filter.Gt != null)
            query = query.Where(BuildBinary(selector, ExpressionType.GreaterThan, filter.Gt));

        if (filter.Gte != null)
            query = query.Where(BuildBinary(selector, ExpressionType.GreaterThanOrEqual, filter.Gte));

        if (filter.Lt != null)
            query = query.Where(BuildBinary(selector, ExpressionType.LessThan, filter.Lt));

        if (filter.Lte != null)
            query = query.Where(BuildBinary(selector, ExpressionType.LessThanOrEqual, filter.Lte));

        return query;
    }


    public static IQueryable<T> ApplyIntFilter<T>(
        this IQueryable<T> query,
        FilterQueryParameterDeconstructed<int?>? filter,
        Expression<Func<T, int?>> selector)
    {
        if (filter == null)
            return query;

        if (filter.Eq != null)
            query = query.Where(BuildBinary(selector, ExpressionType.Equal, filter.Eq));

        if (filter.Neq != null)
            query = query.Where(BuildBinary(selector, ExpressionType.NotEqual, filter.Neq));

        if (filter.Gt != null)
            query = query.Where(BuildBinary(selector, ExpressionType.GreaterThan, filter.Gt));

        if (filter.Gte != null)
            query = query.Where(BuildBinary(selector, ExpressionType.GreaterThanOrEqual, filter.Gte));

        if (filter.Lt != null)
            query = query.Where(BuildBinary(selector, ExpressionType.LessThan, filter.Lt));

        if (filter.Lte != null)
            query = query.Where(BuildBinary(selector, ExpressionType.LessThanOrEqual, filter.Lte));

        return query;
    }

    public static IQueryable<T> ApplyGuidFilter<T>(
    this IQueryable<T> query,
    FilterQueryParameterDeconstructed<Guid?>? filter,
    Expression<Func<T, Guid?>> selector)
    {
        if (filter is null)
            return query;

        if (filter.Eq.HasValue)
            query = query.Where(BuildBinary(selector, ExpressionType.Equal, filter.Eq.Value));

        if (filter.Neq.HasValue)
            query = query.Where(BuildBinary(selector, ExpressionType.NotEqual, filter.Neq.Value));

        return query;
    }

    public static IQueryable<T> ApplyBoolFilter<T>(
        this IQueryable<T> query,
        FilterQueryParameterDeconstructed<bool?>? filter,
        Expression<Func<T, bool?>> selector)
    {
        if (filter == null)
            return query;

        if (filter.Eq != null)
            query = query.Where(BuildBinary(selector, ExpressionType.Equal, filter.Eq));

        if (filter.Neq != null)
            query = query.Where(BuildBinary(selector, ExpressionType.NotEqual, filter.Neq));

        return query;
    }

    private static Expression<Func<T, bool>> BuildBinary<T, TValue>(
        Expression<Func<T, TValue>> selector,
        ExpressionType comparisonType,
        TValue? value)
    {
        var parameter = selector.Parameters[0];
        var left = selector.Body;
        var right = Expression.Constant(value, typeof(TValue));

        var body = Expression.MakeBinary(comparisonType, left, right);

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
