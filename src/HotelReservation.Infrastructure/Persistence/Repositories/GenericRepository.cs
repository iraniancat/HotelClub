// مسیر: src/HotelReservation.Infrastructure/Persistence/Repositories/GenericRepository.cs
using HotelReservation.Application.Contracts.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HotelReservation.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly AppDbContext _dbContext;

    public GenericRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual Task UpdateAsync(T entity)
    {
        // <<-- اصلاح کلیدی در اینجا -->>
        // استفاده از متد Update خود DbSet. این متد به صورت هوشمند
        // موجودیت را Attach کرده و وضعیت آن را به Modified تغییر می‌دهد.
        // این روش از بروز خطای "already tracked" جلوگیری کرده و امن‌تر است.
        _dbContext.Set<T>().Update(entity);
        return Task.CompletedTask;
    }

    // ... سایر متدهای شما (AddAsync, GetByIdAsync, و ...) بدون تغییر باقی می‌مانند ...
    public virtual async Task<T?> GetByIdAsync(Guid id, bool asNoTracking = true)
    {
        if (!asNoTracking)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }
        var keyName = GetPrimaryKeyName();
        if (string.IsNullOrEmpty(keyName) || GetPrimaryKeyProperty().ClrType != typeof(Guid))
        {
            throw new InvalidOperationException($"موجودیت '{typeof(T).Name}' کلید اصلی از نوع Guid ندارد یا کلید اصلی پیدا نشد.");
        }
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, keyName);
        var constant = Expression.Constant(id);
        var equality = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);
        return await _dbContext.Set<T>().AsNoTracking().FirstOrDefaultAsync(lambda);
    }
    public virtual async Task<T?> GetByStringIdAsync(string id, bool asNoTracking = true)
    {
        if (!asNoTracking)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }
        var keyName = GetPrimaryKeyName();
        if (string.IsNullOrEmpty(keyName) || GetPrimaryKeyProperty().ClrType != typeof(string))
        {
            throw new InvalidOperationException($"موجودیت '{typeof(T).Name}' کلید اصلی از نوع string ندارد یا کلید اصلی پیدا نشد.");
        }
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, keyName);
        var constant = Expression.Constant(id);
        var equality = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<T, bool>>(equality, parameter);
        return await _dbContext.Set<T>().AsNoTracking().FirstOrDefaultAsync(lambda);
    }
    private IProperty GetPrimaryKeyProperty()
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(T));
        var primaryKey = entityType?.FindPrimaryKey();
        if (primaryKey == null || primaryKey.Properties.Count != 1)
        {
            throw new InvalidOperationException($"موجودیت از نوع '{typeof(T).Name}' باید دقیقاً یک کلید اصلی داشته باشد.");
        }
        return primaryKey.Properties[0];
    }
    private string GetPrimaryKeyName()
    {
        return GetPrimaryKeyProperty().Name;
    }
    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbContext.Set<T>().AddAsync(entity);
        return entity;
    }
    public virtual Task DeleteAsync(T entity)
    {
        _dbContext.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }
    public virtual async Task<IReadOnlyList<T>> GetAllAsync() => await _dbContext.Set<T>().AsNoTracking().ToListAsync();
    public virtual async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate) => await _dbContext.Set<T>().Where(predicate).AsNoTracking().ToListAsync();
    public virtual IQueryable<T> GetQueryable() => _dbContext.Set<T>();
    public virtual async Task DeleteByIdAsync(Guid id)
    {
        var entity = await GetByIdAsync(id, asNoTracking: false);
        if (entity != null)
        {
            await DeleteAsync(entity);
        }
    }
    public virtual async Task DeleteByStringIdAsync(string id)
    {
        var entity = await GetByStringIdAsync(id, asNoTracking: false);
        if (entity != null)
        {
            await DeleteAsync(entity);
        }
    }
    public virtual async Task<IReadOnlyList<T>> GetAsyncWithIncludeString(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, string? includeString = null, bool disableTracking = true)
    {
        IQueryable<T> query = _dbContext.Set<T>();
        if (disableTracking) query = query.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(includeString)) query = query.Include(includeString);
        if (predicate != null) query = query.Where(predicate);
        if (orderBy != null) return await orderBy(query).ToListAsync();
        return await query.ToListAsync();
    }
    public virtual async Task<IReadOnlyList<T>> GetAsyncWithIncludes(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null, List<Expression<Func<T, object>>>? includes = null, bool disableTracking = true)
    {
        IQueryable<T> query = _dbContext.Set<T>();
        if (disableTracking) query = query.AsNoTracking();
        if (includes != null) query = includes.Aggregate(query, (current, include) => current.Include(include));
        if (predicate != null) query = query.Where(predicate);
        if (orderBy != null) return await orderBy(query).ToListAsync();
        return await query.ToListAsync();
    }
}
