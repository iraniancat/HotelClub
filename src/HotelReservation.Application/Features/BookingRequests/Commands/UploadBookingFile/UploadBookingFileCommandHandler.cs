// src/HotelReservation.Application/Features/BookingRequests/Commands/UploadBookingFile/UploadBookingFileCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Infrastructure; // برای IFileStorageService
using HotelReservation.Application.Contracts.Security;    // برای ICurrentUserService
using HotelReservation.Application.DTOs.Booking;          // برای BookingFileDto
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization; // برای IAuthorizationService
using Microsoft.Extensions.Logging;
using System;
using System.IO; // برای Path
using System.Security.Claims; // برای ساخت ClaimsPrincipal موقت
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic; // برای List<Claim>

namespace HotelReservation.Application.Features.BookingRequests.Commands.UploadBookingFile;

public class UploadBookingFileCommandHandler : IRequestHandler<UploadBookingFileCommand, BookingFileDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService; // برای بررسی مجوز
    private readonly ILogger<UploadBookingFileCommandHandler> _logger;

    // نام Policy یا Requirement برای بررسی مجوز آپلود فایل به رزرو
    // می‌توان از همان "CanViewBookingRequest" استفاده کرد یا یک Policy جدید تعریف کرد.
    // فعلاً فرض می‌کنیم اگر کاربر بتواند رزرو را ببیند، می‌تواند فایل هم آپلود کند.
    private const string AuthorizePolicyToManageBooking = "CanViewBookingRequest"; 

    public UploadBookingFileCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        ILogger<UploadBookingFileCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BookingFileDto> Handle(UploadBookingFileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting to upload file '{FileName}' for BookingRequestId: {BookingRequestId} by User: {UserId}",
            request.FileName, request.BookingRequestId, request.UploadedByUserId);

        // ۱. واکشی درخواست رزرو
        var bookingRequest = await _unitOfWork.BookingRequestRepository.GetByIdAsync(request.BookingRequestId);
        if (bookingRequest == null)
        {
            _logger.LogWarning("UploadFile: BookingRequest with ID {BookingRequestId} not found.", request.BookingRequestId);
            throw new NotFoundException(nameof(BookingRequest), request.BookingRequestId);
        }

        // ۲. بررسی مجوز کاربر برای آپلود فایل به این درخواست رزرو
        // از همان Policy که برای مشاهده جزئیات رزرو استفاده کردیم، بهره می‌بریم.
        // اگر نیاز به منطق دقیق‌تری بود، باید یک Policy/Requirement جدید تعریف شود.
        var userPrincipal = _currentUserService.GetUserPrincipal(); // فرض می‌کنیم ICurrentUserService این را برمی‌گرداند
        if (userPrincipal == null) // اگر کاربر احراز هویت نشده باشد (که با [Authorize] در کنترلر باید گرفته شود)
        {
             _logger.LogWarning("UploadFile: User performing upload is not authenticated. BookingRequestId: {BookingRequestId}", request.BookingRequestId);
            throw new UnauthorizedAccessException("کاربر برای آپلود فایل احراز هویت نشده است.");
        }
         // اطمینان از اینکه UploadedByUserId با کاربر فعلی یکی است
        if(_currentUserService.UserId != request.UploadedByUserId)
        {
             _logger.LogWarning("UploadFile: Mismatch between authenticated user ({CurrentUserId}) and UploadedByUserId in command ({CommandUserId}) for BookingRequestId: {BookingRequestId}.", 
                 _currentUserService.UserId, request.UploadedByUserId, request.BookingRequestId);
            throw new ForbiddenAccessException("کاربر آپلود کننده با کاربر احراز هویت شده مطابقت ندارد.");
        }


        var authorizationResult = await _authorizationService.AuthorizeAsync(
            userPrincipal,
            bookingRequest, // منبع: خود درخواست رزرو
            AuthorizePolicyToManageBooking 
        );

        if (!authorizationResult.Succeeded)
        {
            _logger.LogWarning(
                "UploadFile: User {UserId} failed authorization policy '{PolicyName}' for BookingRequest {BookingRequestId}.",
                request.UploadedByUserId, AuthorizePolicyToManageBooking, request.BookingRequestId);
            throw new ForbiddenAccessException("شما مجاز به آپلود فایل برای این درخواست رزرو نیستید.");
        }
        _logger.LogInformation("UploadFile: User {UserId} authorized for BookingRequest {BookingRequestId}.", request.UploadedByUserId, request.BookingRequestId);


        // ۳. ذخیره فایل با استفاده از سرویس ذخیره‌سازی فایل
        // زیرپوشه می‌تواند بر اساس شناسه درخواست رزرو یا تاریخ باشد
        string fileSubFolder = $"booking_{request.BookingRequestId}"; 
        string storedFilePathIdentifier = await _fileStorageService.SaveFileAsync(request.FileStream, request.FileName, fileSubFolder);

        // ۴. ایجاد موجودیت BookingFile
        var bookingFileEntity = new BookingFile(
            request.BookingRequestId,
            bookingRequest, // Navigation property
            Path.GetFileName(request.FileName), // فقط نام فایل بدون مسیر احتمالی از کلاینت
            storedFilePathIdentifier, // مسیری که از FileStorageService برگردانده شده
            request.ContentType
        );

        // ۵. افزودن به پایگاه داده
        // اگر BookingRequest.Files یک ICollection است، می‌توانیم مستقیماً به آن اضافه کنیم
        // bookingRequest.Files.Add(bookingFileEntity); 
        // await _unitOfWork.BookingRequestRepository.UpdateAsync(bookingRequest); // برای ذخیره تغییر در کالکشن
        // یا اگر BookingFile یک DbSet جداگانه دارد و رابطه به درستی تنظیم شده:
        await _unitOfWork.BookingFileRepository.AddAsync(bookingFileEntity); // استفاده از Generic Repository

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("File record created in DB for BookingRequestId: {BookingRequestId}, FileId: {FileId}, Path: {FilePath}", 
            request.BookingRequestId, bookingFileEntity.Id, storedFilePathIdentifier);

        // ۶. نگاشت به DTO و بازگرداندن نتیجه
        return new BookingFileDto
        {
            Id = bookingFileEntity.Id,
            FileName = bookingFileEntity.FileName,
            ContentType = bookingFileEntity.ContentType,
            UploadedDate = bookingFileEntity.UploadedDate,
            // DownloadUrl فعلاً null خواهد بود چون فایل‌ها مستقیماً از وب قابل دسترس نیستند
            // این URL باید به یک Endpoint دانلود در Controller اشاره کند.
            DownloadUrl = _fileStorageService.GetFileUrl(storedFilePathIdentifier) ?? $"/api/booking-requests/{request.BookingRequestId}/files/{bookingFileEntity.Id}/download" // URL فرضی
        };
    }
}