// src/HotelReservation.Application/Contracts/Infrastructure/IFileStorageService.cs
using Microsoft.AspNetCore.Http; // برای IFormFile (یا می‌توانیم Stream استفاده کنیم)
using System.IO; // برای Stream
using System.Threading.Tasks;

namespace HotelReservation.Application.Contracts.Infrastructure;

public interface IFileStorageService
{
    /// <summary>
    /// فایل را در محل ذخیره‌سازی ذخیره کرده و مسیر یا شناسه ذخیره شده آن را برمی‌گرداند.
    /// </summary>
    /// <param name="fileStream">استریم فایل.</param>
    /// <param name="originalFileName">نام اصلی فایل برای استخراج پسوند و ساخت نام جدید.</param>
    /// <param name="subFolder">زیرپوشه‌ای که فایل باید در آن ذخیره شود (مثلاً "booking_attachments").</param>
    /// <returns>مسیر نسبی یا شناسه فایل ذخیره شده.</returns>
    Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string subFolder);

    /// <summary>
    /// فایل را بر اساس مسیر یا شناسه آن می‌خواند.
    /// </summary>
    /// <param name="filePathOrIdentifier">مسیر یا شناسه فایل.</param>
    /// <returns>آرایه بایت محتوای فایل، یا null اگر یافت نشود.</returns>
    Task<byte[]?> GetFileAsync(string filePathOrIdentifier);
    
    /// <summary>
    /// URL برای دسترسی به فایل را برمی‌گرداند (اگر فایل‌ها به صورت عمومی قابل دسترس باشند).
    /// </summary>
    /// <param name="filePathOrIdentifier">مسیر یا شناسه فایل.</param>
    /// <returns>URL دسترسی به فایل یا null.</returns>
    string? GetFileUrl(string filePathOrIdentifier);


    /// <summary>
    /// فایل را بر اساس مسیر یا شناسه آن حذف می‌کند.
    /// </summary>
    /// <param name="filePathOrIdentifier">مسیر یا شناسه فایل.</param>
    Task DeleteFileAsync(string filePathOrIdentifier);
}