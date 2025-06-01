// src/HotelReservation.Application/Features/BookingRequests/Commands/UploadBookingFile/UploadBookingFileCommandValidator.cs
using FluentValidation;
using System;
using System.Collections.Generic; // برای لیست انواع مجاز فایل
using System.Linq; // برای Contains

namespace HotelReservation.Application.Features.BookingRequests.Commands.UploadBookingFile;

public class UploadBookingFileCommandValidator : AbstractValidator<UploadBookingFileCommand>
{
    // تعریف انواع فایل مجاز و حداکثر اندازه فایل (مثال)
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly List<string> AllowedContentTypes = new List<string> 
    { 
        "application/pdf", 
        "application/msword", // .doc
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document" // .docx
        // سایر انواع مجاز مانند "image/jpeg", "image/png" در صورت نیاز
    };
     private static readonly List<string> AllowedExtensions = new List<string> 
    { 
        ".pdf", 
        ".doc",
        ".docx"
    };


    public UploadBookingFileCommandValidator()
    {
        RuleFor(p => p.BookingRequestId)
            .NotEmpty().WithMessage("شناسه درخواست رزرو الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه درخواست رزرو معتبر نیست.");

        RuleFor(p => p.UploadedByUserId)
            .NotEmpty().WithMessage("شناسه کاربر آپلود کننده الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه کاربر آپلود کننده معتبر نیست.");

        RuleFor(p => p.FileName)
            .NotEmpty().WithMessage("نام فایل الزامی است.")
            .MaximumLength(255).WithMessage("نام فایل نمی‌تواند بیشتر از ۲۵۵ کاراکتر باشد.")
            .Must(HaveAllowedExtension).WithMessage("پسوند فایل مجاز نیست. فقط فایل‌های Word و PDF مجاز هستند.");


        RuleFor(p => p.ContentType)
            .NotEmpty().WithMessage("نوع محتوای فایل الزامی است.")
            .Must(BeAllowedContentType).WithMessage("نوع محتوای فایل مجاز نیست. فقط فایل‌های Word و PDF مجاز هستند.");
            

        RuleFor(p => p.FileStream)
            .NotNull().WithMessage("محتوای فایل الزامی است.")
            .Must(stream => stream.Length > 0).WithMessage("فایل نمی‌تواند خالی باشد.")
            .Must(stream => stream.Length <= MaxFileSizeInBytes)
                .WithMessage($"حداکثر اندازه مجاز برای فایل {MaxFileSizeInBytes / 1024 / 1024} مگابایت است.");
    }

    private bool BeAllowedContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType)) return false; // اگر اختیاری بود، این بررسی متفاوت می‌شد
        return AllowedContentTypes.Contains(contentType.ToLowerInvariant());
    }
    private bool HaveAllowedExtension(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;
        var extension = System.IO.Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension)) return false; // فاقد پسوند
        return AllowedExtensions.Contains(extension);
    }
}