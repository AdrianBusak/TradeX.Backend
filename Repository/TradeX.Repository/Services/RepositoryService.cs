using TradeX.Domain.Abstractions.Interfaces;
using TradeX.Repository.Abstractions.Enums;
using TradeX.Repository.Abstractions.Extensions;
using TradeX.Repository.Abstractions.Interfaces;
using TradeX.Repository.Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq.Expressions;

namespace TradeX.Repository.Services;

public class RepositoryService<TDbContext>(TDbContext dbContext, ILogger<RepositoryService<TDbContext>> logger)
    : IRepository<TDbContext> where TDbContext : DbContext
{
    private readonly TDbContext _dbContext = dbContext;
    private readonly ILogger<RepositoryService<TDbContext>> _logger = logger;

    public TDbContext DbContext => _dbContext;

    #region Query

    public async Task<bool> ExistsAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        return await _dbContext.Set<T>()
            .AsNoTracking()
            .AnyAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<T?> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        return await _dbContext.Set<T>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<T?> GetSingleAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        return await GetSingleAsync<T>(cancellationToken, [predicate]).ConfigureAwait(false);
    }

    public async Task<T?> GetSingleAsync<T>(CancellationToken cancellationToken = default,
        params Expression<Func<T, bool>>[] predicates)
        where T : class, IBaseEntity
    {
        var queryable = ApplyExpressions(_dbContext.Set<T>(), predicates);
        return await queryable.AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
    public async Task<List<T>> GetListAsync<T>(CancellationToken cancellationToken = default,
        params Expression<Func<T, bool>>[] predicates)
        where T : class, IBaseEntity
    {
        var queryable = ApplyExpressions(_dbContext.Set<T>(), predicates);
        return await ExecuteSimpleQueryAsync(queryable, -1, 0, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<T>> GetListAsync<T, TKey>(
        Expression<Func<T, TKey>>? orderByAsc = null,
        Expression<Func<T, TKey>>? orderByDesc = null,
        int maxRecords = -1,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, bool>>[] predicates)
        where T : class, IBaseEntity
    {
        var queryable = ApplyExpressions(_dbContext.Set<T>(), predicates);

        if (orderByAsc != null)
            queryable = queryable.OrderBy(orderByAsc);

        if (orderByDesc != null)
            queryable = queryable.OrderByDescending(orderByDesc);

        return await ExecuteSimpleQueryAsync(queryable, maxRecords, 0, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<T>> GetAllAsync<T>(CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        return await ExecuteSimpleQueryAsync(_dbContext.Set<T>(), -1, 0, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Guid?> GetIdAsync<T>(
        params Expression<Func<T, bool>>[] predicates)
        where T : class, IBaseEntity
    {
        return await GetIdAsync<T>(predicates, CancellationToken.None)
            .ConfigureAwait(false);
    }

    public async Task<Guid?> GetIdAsync<T>(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        return await GetIdAsync<T>([predicate], cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<Guid?> GetIdAsync<T>(
        IEnumerable<Expression<Func<T, bool>>>? predicates = null,
        CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        IQueryable<T> query = _dbContext.Set<T>();

        if (predicates is not null)
        {
            foreach (var predicate in predicates)
                query = query.Where(predicate);
        }

        return await query.AsNoTracking()
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<ExecuteQueryResponse<TResponse>> QueryAsync<TResponse>(
        IQueryable<TResponse> query,
        int pageIndex = 0,
        int pageSize = -1,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        var skipRecords = CalculateDataPagingRecordsToSkip(pageSize, pageIndex);

        var records = await ExecuteSimpleQueryAsync(query, pageSize, skipRecords, cancellationToken)
            .ConfigureAwait(false);

        var totalCount = pageSize != -1
            ? await query.CountAsync(cancellationToken).ConfigureAwait(false)
            : records.Count;

        return new ExecuteQueryResponse<TResponse>
        {
            TotalRecordCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize,
            Records = records
        };
    }

    #endregion

    #region Commands

    public async Task<Guid> AddAsync<T>(T entity, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (entity.Id == Guid.Empty)
        {
            entity.Id = Guid.NewGuid();
        }

        entity.CreatedAt = DateTime.UtcNow;
        entity.ModifiedAt = DateTime.UtcNow;

        _dbContext.Set<T>().Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return entity.Id;
    }

    public async Task AddRangeAsync<T>(List<T> entities, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        ArgumentNullException.ThrowIfNull(entities);

        foreach (var entity in entities)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            entity.CreatedAt = DateTime.UtcNow;
            entity.ModifiedAt = DateTime.UtcNow;
        }

        _dbContext.Set<T>().AddRange(entities);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.ModifiedAt = DateTime.UtcNow;

        _dbContext.Set<T>().Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateRangeAsync<T>(List<T> entities, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        ArgumentNullException.ThrowIfNull(entities);

        _dbContext.Set<T>().UpdateRange(entities);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }


    public async Task<UpsertEntityResult> UpsertAsync<T>(
        T entity, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        ArgumentNullException.ThrowIfNull(entity);

        var existing = await GetByIdAsync<T>(entity.Id, cancellationToken).ConfigureAwait(false);

        if (existing == null)
        {
            await AddAsync(entity, cancellationToken).ConfigureAwait(false);
            return UpsertEntityResult.Inserted;
        }

        _dbContext.DetachLocal(entity, entity.Id);
        await MergeWithExistingAndUpdateEntityAsync(entity, existing, cancellationToken)
            .ConfigureAwait(false);

        return UpsertEntityResult.Updated;
    }

    public async Task DeleteAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        var entity = await GetByIdAsync<T>(id, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Entity not found. [id: {id}]");

        if (entity is not IAuditableEntityWithSoftDelete soft)
            throw new InvalidOperationException($"Entity type {typeof(T).Name} does not support soft delete.");

        soft.IsActive = false;
        _dbContext.Set<T>().Update(entity);

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteRangeAsync<T>(IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        if (ids == null || !ids.Any())
            return;

        var entities = await _dbContext.Set<T>()
            .Where(e => ids.Contains(e.Id))
            .ToListAsync(cancellationToken);

        foreach (var entity in entities.OfType<IAuditableEntityWithSoftDelete>())
        {
            entity.IsActive = false;

            if (entity is IBaseEntity baseEntity)
            {
                baseEntity.ModifiedAt = DateTime.UtcNow;
            }

            _dbContext.Set<T>().Update((T)entity);
        }

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteWhereAsync<T>(
        CancellationToken cancellationToken = default,
        params Expression<Func<T, bool>>[] predicates)
        where T : class, IBaseEntity
    {
        var entities = await ApplyExpressions(_dbContext.Set<T>(), predicates)
            .ToListAsync(cancellationToken);

        foreach (var entity in entities.OfType<IAuditableEntityWithSoftDelete>())
        {
            entity.IsActive = false;

            if (entity is IBaseEntity baseEntity)
            {
                baseEntity.ModifiedAt = DateTime.UtcNow;
            }

            _dbContext.Set<T>().Update((T)entity);
        }

        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteHardAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        int affected = await _dbContext.Set<T>()
            .Where(e => e.Id == id)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);

        if (affected == 0)
            throw new KeyNotFoundException($"Entity not found. [id: {id}]");
    }

    public async Task DeleteHardRangeAsync<T>(IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
        where T : class, IBaseEntity
    {
        if (ids == null || !ids.Any())
            return;

        await _dbContext.Set<T>()
            .Where(e => ids.Contains(e.Id))
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task DeleteHardWhereAsync<T>(
        CancellationToken cancellationToken = default,
        params Expression<Func<T, bool>>[] predicates)
        where T : class, IBaseEntity
    {
        await ApplyExpressions(_dbContext.Set<T>(), predicates)
            .ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    #endregion

    #region Transactions

    public async Task<TransactionModel> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        var transaction = await _dbContext.Database
            .BeginTransactionAsync(isolationLevel, cancellationToken);

        return new TransactionModel(transaction);
    }

    public async Task CommitTransactionAsync(
        TransactionModel transactionModel,
        CancellationToken cancellationToken = default)
    {
        await transactionModel.Transaction!.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(
        TransactionModel transactionModel,
        CancellationToken cancellationToken = default)
    {
        await transactionModel.Transaction!.RollbackAsync(cancellationToken);
    }

    #endregion

    #region Private helpers

    private static IQueryable<T> ApplyExpressions<T>(
        IQueryable<T> query, params Expression<Func<T, bool>>[] predicates)
        where T : class
    {
        foreach (var exp in predicates)
            query = query.Where(exp);

        return query;
    }

    private static async Task<List<TResponse>> ExecuteSimpleQueryAsync<TResponse>(
        IQueryable<TResponse> query,
        int maxRecordCount,
        int skipRecords,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        query = query.AsNoTracking();

        if (skipRecords > 0)
            query = query.Skip(skipRecords);

        if (maxRecordCount != -1)
            query = query.Take(maxRecordCount);

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private static int CalculateDataPagingRecordsToSkip(int pageSize, int pageIndex)
        => pageSize > 0 ? pageIndex * pageSize : 0;

    private async Task MergeWithExistingAndUpdateEntityAsync<T>(
        T entity, T existing, CancellationToken cancellationToken)
        where T : class, IBaseEntity
    {
        if (entity is IAuditableEntityWithSoftDelete s1 &&
            existing is IAuditableEntityWithSoftDelete s2)
        {
            s1.IsActive = s2.IsActive;
        }

        await UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    #endregion
}