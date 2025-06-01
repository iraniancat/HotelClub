// src/HotelReservation.Infrastructure/Services/FileSystemStorageService.cs
using HotelReservation.Application.Contracts.Infrastructure;
using Microsoft.AspNetCore.Hosting; // برای IWebHostEnvironment
using Microsoft.Extensions.Configuration; // برای IConfiguration
using Microsoft.Extensions.Logging; // برای ILogger
using System;
using System.IO;
using System.Threading.Tasks;

namespace HotelReservation.Infrastructure.Services;

public class FileSystemStorageService : IFileStorageService
{
    private readonly string _baseUploadPath; // مسیر کامل پایه برای آپلودها
    private readonly ILogger<FileSystemStorageService> _logger;
    private readonly IConfiguration _configuration; // برای دسترسی به URL پایه برنامه (اگر لازم باشد)

    public FileSystemStorageService(IWebHostEnvironment env, IConfiguration configuration, ILogger<FileSystemStorageService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        var configuredPath = configuration["FileStorageSettings:BaseUploadPath"];
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            _logger.LogError("BaseUploadPath is not configured in FileStorageSettings.");
            throw new InvalidOperationException("مسیر پایه آپلود فایل‌ها پیکربندی نشده است.");
        }
        // ContentRootPath مسیر ریشه برنامه است (جایی که فایل اجرایی و appsettings.json قرار دارند)
        _baseUploadPath = Path.Combine(env.ContentRootPath, configuredPath);

        if (!Directory.Exists(_baseUploadPath))
        {
            try
            {
                Directory.CreateDirectory(_baseUploadPath);
                _logger.LogInformation("Base upload directory created at: {Path}", _baseUploadPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create base upload directory at: {Path}", _baseUploadPath);
                throw; // دوباره پرتاب خطا تا برنامه متوجه مشکل شود
            }
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string subFolder)
    {
        if (fileStream == null || fileStream.Length == 0)
        {
            throw new ArgumentException("استریم فایل نمی‌تواند خالی باشد.", nameof(fileStream));
        }
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            throw new ArgumentException("نام اصلی فایل نمی‌تواند خالی باشد.", nameof(originalFileName));
        }
        if (string.IsNullOrWhiteSpace(subFolder))
        {
            throw new ArgumentException("نام زیرپوشه نمی‌تواند خالی باشد.", nameof(subFolder));
        }

        var targetDirectory = Path.Combine(_baseUploadPath, subFolder);
        if (!Directory.Exists(targetDirectory))
        {
            try
            {
                Directory.CreateDirectory(targetDirectory);
                _logger.LogInformation("Subfolder directory created at: {Path}", targetDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create subfolder directory at: {Path}", targetDirectory);
                throw;
            }
        }

        // ایجاد یک نام فایل یکتا برای جلوگیری از تداخل و مسائل امنیتی
        var fileExtension = Path.GetExtension(originalFileName).ToLowerInvariant(); // پسوند فایل با حروف کوچک
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var fullFilePath = Path.Combine(targetDirectory, uniqueFileName);

        try
        {
            await using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                fileStream.Seek(0, SeekOrigin.Begin); // اطمینان از اینکه استریم از ابتدا خوانده می‌شود
                await fileStream.CopyToAsync(stream);
            }
            _logger.LogInformation("File saved successfully: {FilePath}", fullFilePath);

            // مسیر نسبی برای ذخیره در پایگاه داده (نسبت به BaseUploadPath مفهومی ما)
            return Path.Combine(subFolder, uniqueFileName).Replace('\\', '/'); // استفاده از / برای سازگاری مسیرها
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file {FileName} to {FilePath}", originalFileName, fullFilePath);
            throw new ApplicationException($"خطا در ذخیره‌سازی فایل '{originalFileName}'.", ex);
        }
    }

    public async Task<byte[]?> GetFileAsync(string filePathOrIdentifier)
    {
        if (string.IsNullOrWhiteSpace(filePathOrIdentifier)) return null;

        // filePathOrIdentifier باید مسیر نسبی باشد که توسط SaveFileAsync برگردانده شده
        var fullFilePath = Path.Combine(_baseUploadPath, filePathOrIdentifier.Replace('/', Path.DirectorySeparatorChar));

        try
        {
            if (File.Exists(fullFilePath))
            {
                return await File.ReadAllBytesAsync(fullFilePath);
            }
            _logger.LogWarning("File not found for retrieval: {FilePath}", fullFilePath);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving file from {FilePath}", fullFilePath);
            return null; // یا پرتاب خطا بسته به نیاز
        }
    }
    
    public string? GetFileUrl(string filePathOrIdentifier)
    {
         // از آنجایی که فایل‌ها خارج از wwwroot ذخیره می‌شوند، URL مستقیمی برای آن‌ها وجود ندارد.
         // برای دسترسی به این فایل‌ها، باید یک Endpoint جداگانه در Controller ایجاد شود
         // که فایل را بخواند و به صورت Stream به کلاینت ارسال کند (با بررسی مجوزهای لازم).
         // فعلاً null برمی‌گردانیم یا می‌توانیم یک مسیر به آن Endpoint فرضی برگردانیم.
         _logger.LogInformation("GetFileUrl called for {FilePathOrIdentifier}. Direct URL access for files outside wwwroot is not provided by this service. A dedicated download endpoint is needed.", filePathOrIdentifier);
         return null; 
         // مثال اگر در wwwroot بود:
         // if (string.IsNullOrWhiteSpace(filePathOrIdentifier)) return null;
         // var relativeUrl = $"/{_configuration["FileStorageSettings:BaseUploadPath"]}/{filePathOrIdentifier}".Replace("\\", "/");
         // return relativeUrl;
    }

    public Task DeleteFileAsync(string filePathOrIdentifier)
    {
        if (string.IsNullOrWhiteSpace(filePathOrIdentifier)) return Task.CompletedTask;

        var fullFilePath = Path.Combine(_baseUploadPath, filePathOrIdentifier.Replace('/', Path.DirectorySeparatorChar));

        try
        {
            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);
                _logger.LogInformation("File deleted successfully: {FilePath}", fullFilePath);
            }
            else
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", fullFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from {FilePath}", fullFilePath);
            // تصمیم بگیرید که آیا در صورت خطا، Exception پرتاب شود یا خیر
        }
        return Task.CompletedTask;
    }
}