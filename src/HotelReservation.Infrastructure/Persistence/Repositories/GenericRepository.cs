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
     public virtual async Task<T> AddAsync(T entity)
    {
        if (entity == null)
        {
            Console.WriteLine($"[DEBUG] GenericRepository.AddAsync for {typeof(T).Name}: Received a null entity.");
            throw new ArgumentNullException(nameof(entity));
        }

        var entityType = typeof(T).Name;
        var entryStateBeforeAdd = _dbContext.Entry(entity).State;
        Console.WriteLine($"[DEBUG] GenericRepository.AddAsync for {entityType}. DbContext HashCode: {_dbContext.GetHashCode()}. Entity State BEFORE _dbContext.Set<T>().AddAsync(entity): {entryStateBeforeAdd}");
        
        await _dbContext.Set<T>().AddAsync(entity); // این خط باید وضعیت را به Added تغییر دهد
        
        var entryStateAfterAdd = _dbContext.Entry(entity).State;
        Console.WriteLine($"[DEBUG] GenericRepository.AddAsync for {entityType}. DbContext HashCode: {_dbContext.GetHashCode()}. Entity State AFTER _dbContext.Set<T>().AddAsync(entity): {entryStateAfterAdd}");
        
        // یک بررسی اضافی برای اطمینان
        if (entryStateAfterAdd != EntityState.Added)
        {
            Console.WriteLine($"[ERROR_DEBUG] GenericRepository.AddAsync for {entityType}: Entity state is NOT Added after AddAsync call. It is {entryStateAfterAdd}. This is a problem!");
        }
        
        return entity;
    }

      public virtual Task UpdateAsync(T entity)
    {
        // برای اطمینان، وضعیت را صریحاً Modified می‌کنیم
        _dbContext.Entry(entity).State = EntityState.Modified;
        Console.WriteLine($"[DEBUG] GenericRepository.UpdateAsync for {typeof(T).Name}. DbContext HashCode: {_dbContext.GetHashCode()}. Entity State set to Modified.");
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
        Console.WriteLine($"[DEBUG] GenericRepository.DeleteAsync for {typeof(T).Name}. DbContext HashCode: {_dbContext.GetHashCode()}. Entity State after Remove: {_dbContext.Entry(entity).State}"); // باید Deleted باشد
        return Task.CompletedTask;
    }
    public virtual async Task DeleteByIdAsync(Guid id) 
    {
        var entity = await GetByIdAsync(id);
        if (entity != null) await DeleteAsync(entity);
    }
    public virtual async Task DeleteByStringIdAsync(string id) 
    {
        var entity = await GetByStringIdAsync(id);
        if (entity != null) await DeleteAsync(entity);
    }

  
}