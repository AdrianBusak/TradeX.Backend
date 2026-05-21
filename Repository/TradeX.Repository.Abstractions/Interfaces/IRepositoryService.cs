using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TradeX.Domain.Abstractions.Interfaces;
using TradeX.Repository.Abstractions.Enums;
using TradeX.Repository.Abstractions.Models;

namespace TradeX.Repository.Abstractions.Interfaces;

public interface IRepository<TDbContext> where TDbContext : DbContext
{
    TDbContext DbContext { get; }

    Task<bool> ExistsAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;

    Task<T?> GetByIdAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;

    Task<T?> GetSingleAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;

    Task<T?> GetSingleAsync<T>(CancellationToken cancellationToken = default,
        params Expression<Func<T, bool>>[] predicates)
        where T : class, IBaseEntity;

    Task<List<T>> GetListAsync<T>(CancellationToken cancellationToken = default,
        params Expression<Func<T, bool>>[] predicates)
        where T : class, IBaseEntity;

    Task<List<T>> GetListAsync<T, TKey>(
        Expression<Func<T, TKey>>? orderByAsc = null,
        Expression<Func<T, TKey>>? orderByDesc = null,
        int maxRecords = -1,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, bool>>[] predicates)
        where T : class, IBaseEntity;

    Task<List<T>> GetAllAsync<T>(CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;

    Task<Guid?> GetIdAsync<T>(params Expression<Func<T, bool>>[] predicates)
        where T : class, IBaseEntity;
    Task<Guid?> GetIdAsync<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;
    Task<Guid?> GetIdAsync<T>(IEnumerable<Expression<Func<T, bool>>>? predicates = null, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;

    Task<ExecuteQueryResponse<TResponse>> QueryAsync<TResponse>(
        IQueryable<TResponse> query,
        int pageIndex = 0,
        int pageSize = -1,
        CancellationToken cancellationToken = default)
        where TResponse : class;
    Task<Guid> AddAsync<T>(T entity, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;

    Task AddRangeAsync<T>(List<T> entities, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;

    Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;
    Task UpdateRangeAsync<T>(List<T> entities, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;

    Task<UpsertEntityResult> UpsertAsync<T>(
        T entity, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;

    Task DeleteAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;

    Task DeleteRangeAsync<T>(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;

    Task DeleteWhereAsync<T>(CancellationToken cancellationToken = default,
        params Expression<Func<T, bool>>[] predicates)
        where T : class, IBaseEntity;

    Task DeleteHardAsync<T>(Guid id, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;

    Task DeleteHardRangeAsync<T>(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        where T : class, IBaseEntity;

    Task DeleteHardWhereAsync<T>(CancellationToken cancellationToken = default,
        params Expression<Func<T, bool>>[] predicates)
        where T : class, IBaseEntity;

    Task<TransactionModel> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(
        TransactionModel transaction,
        CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(
        TransactionModel transaction,
        CancellationToken cancellationToken = default);
}
