// src/HotelReservation.Infrastructure/Persistence/Repositories/GenericRepository.cs
using HotelReservation.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HotelReservation.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _dbContext;

    public GenericRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    // ... (سایر متدها مانند GetByIdAsync, GetAllAsync, GetAsync(predicate) بدون تغییر) ...

    public virtual async Task<T?> GetByIdAsync(Guid id) => await _dbContext.Set<T>().FindAsync(id);
    public virtual async Task<T?> GetByStringIdAsync(string id) => await _dbContext.Set<T>().FindAsync(id);
    public virtual async Task<IReadOnlyList<T>> GetAllAsync() => await _dbContext.Set<T>().AsNoTracking().ToListAsync();
    public virtual async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate) => await _dbContext.Set<T>().Where(predicate).AsNoTracking().ToListAsync();

    public virtual IQueryable<T> GetQueryable()
    {
        return _dbContext.Set<T>();
    }
    // پیاده‌سازی با نام جدید
    public virtual async Task<IReadOnlyList<T>> GetAsyncWithIncludeString(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string? includeString = null,
        bool disableTracking = true)
    {
        IQueryable<T> query = _dbContext.Set<T>();
        if (disableTracking) query = query.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(includeString)) query = query.Include(includeString);
        if (predicate != null) query = query.Where(predicate);
        if (orderBy != null) return await orderBy(query).ToListAsync();
        return await query.ToListAsync();
    }

    // پیاده‌سازی با نام جدید
    public virtual async Task<IReadOnlyList<T>> GetAsyncWithIncludes(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        List<Expression<Func<T, object>>>? includes = null,
        bool disableTracking = true)
    {
        IQueryable<T> query = _dbContext.Set<T>();
        if (disableTracking) query = query.AsNoTracking();
        if (includes != null) query = includes.Aggregate(query, (current, include) => current.Include(include));
        if (predicate != null) query = query.Where(predicate);
        if (orderBy != null) return await orderBy(query).ToListAsync();
        return await query.ToListAsync();
    }

    // ... (متدهای AddAsync, UpdateAsync, DeleteAsync, DeleteByIdAsync, DeleteByStringIdAsync بدون تغییر) ...
    public virtual async Task<T> AddAsync(T entity) { /*...*/ return entity; }
    public virtual Task UpdateAsync(T entity) { /*...*/ return Task.CompletedTask; }
    public virtual Task DeleteAsync(T entity) { /*...*/ return Task.CompletedTask; }
    public virtual async Task DeleteByIdAsync(Guid id) { /*...*/ }
    public virtual async Task DeleteByStringIdAsync(string id) { /*...*/ }

  
}