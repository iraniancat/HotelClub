// src/HotelReservation.Application/Contracts/Persistence/IGenericRepository.cs
using System.Linq.Expressions;

namespace HotelReservation.Application.Contracts.Persistence;

public interface IGenericRepository<T> where T : class
{

    Task<T?> GetByIdAsync(Guid id);
    Task<T?> GetByStringIdAsync(string id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> predicate); // این متد ساده باقی می‌ماند

    // تغییر نام برای رفع ابهام
    Task<IReadOnlyList<T>> GetAsyncWithIncludeString( // <<-- تغییر نام
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        string? includeString = null,
        bool disableTracking = true);

    // تغییر نام برای رفع ابهام
    Task<IReadOnlyList<T>> GetAsyncWithIncludes( // <<-- تغییر نام
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        List<Expression<Func<T, object>>>? includes = null,
        bool disableTracking = true);

    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task DeleteByIdAsync(Guid id);
    Task DeleteByStringIdAsync(string id);
    IQueryable<T> GetQueryable();
}