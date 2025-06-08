// src/HotelReservation.Application/Features/BookingRequests/Commands/DeleteBookingFile/DeleteBookingFileCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Infrastructure; // برای IFileStorageService
using HotelReservation.Application.Contracts.Security;    // برای ICurrentUserService
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization; // برای IAuthorizationService
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.BookingRequests.Commands.DeleteBookingFile;

public class DeleteBookingFileCommandHandler : IRequestHandler<DeleteBookingFileCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<DeleteBookingFileCommandHandler> _logger;

    // همان Policy که برای مشاهده جزئیات رزرو استفاده کردیم، می‌تواند برای مدیریت فایل‌های آن نیز استفاده شود.
    // یا می‌توانید یک Policy خاص‌تر تعریف کنید.
    private const string AuthorizePolicyToManageBookingFiles = "CanViewBookingRequest"; 

    public DeleteBookingFileCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        ILogger<DeleteBookingFileCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(DeleteBookingFileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting to delete FileId: {FileId} from BookingRequestId: {BookingRequestId} by User: {UserId}",
            request.FileId, request.BookingRequestId, _currentUserService.UserId);

        // ۱. واکشی درخواست رزرو برای بررسی مجوز
        var bookingRequest = await _unitOfWork.BookingRequestRepository.GetByIdAsync(request.BookingRequestId);
        if (bookingRequest == null)
        {
            _logger.LogWarning("DeleteBookingFile: BookingRequest with ID {BookingRequestId} not found.", request.BookingRequestId);
            throw new NotFoundException(nameof(BookingRequest), request.BookingRequestId);
        }

        // ۲. بررسی مجوز کاربر برای این عملیات روی این درخواست رزرو
        var userPrincipal = _currentUserService.GetUserPrincipal();
        if (userPrincipal == null) {
            _logger.LogWarning("DeleteBookingFile: User performing delete is not authenticated. BookingRequestId: {BookingRequestId}", request.BookingRequestId);
            throw new UnauthorizedAccessException("کاربر برای حذف فایل احراز هویت نشده است.");
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(
            userPrincipal,
            bookingRequest, // منبع: خود درخواست رزرو
            AuthorizePolicyToManageBookingFiles 
        );

        if (!authorizationResult.Succeeded)
        {
            _logger.LogWarning(
                "DeleteBookingFile: User {UserId} failed authorization policy '{PolicyName}' for BookingRequest {BookingRequestId}.",
                _currentUserService.UserId, AuthorizePolicyToManageBookingFiles, request.BookingRequestId);
            throw new ForbiddenAccessException("شما مجاز به حذف فایل‌های این درخواست رزرو نیستید.");
        }
        _logger.LogInformation("DeleteBookingFile: User {UserId} authorized for BookingRequest {BookingRequestId}.", 
            _currentUserService.UserId, request.BookingRequestId);

        // ۳. واکشی اطلاعات فایل از پایگاه داده
        var fileToDelete = await _unitOfWork.BookingFileRepository.GetByIdAsync(request.FileId);
        if (fileToDelete == null || fileToDelete.BookingRequestId != request.BookingRequestId)
        {
            _logger.LogWarning("DeleteBookingFile: File with ID {FileId} not found or does not belong to BookingRequest {BookingRequestId}.", 
                request.FileId, request.BookingRequestId);
            throw new NotFoundException($"فایل با شناسه '{request.FileId}' برای این درخواست رزرو یافت نشد.", nameof(BookingFile));
        }

        // ۴. حذف فایل فیزیکی از سیستم ذخیره‌سازی
        try
        {
            await _fileStorageService.DeleteFileAsync(fileToDelete.FilePathOrContentIdentifier);
            _logger.LogInformation("DeleteBookingFile: Physical file '{FilePath}' deleted successfully for FileId: {FileId}.", 
                fileToDelete.FilePathOrContentIdentifier, request.FileId);
        }
        catch (Exception ex)
        {
            // حتی اگر حذف فیزیکی فایل با خطا مواجه شد، شاید بخواهیم رکورد دیتابیس را حذف کنیم
            // یا خطا را لاگ کرده و به کاربر اطلاع دهیم. فعلاً خطا را لاگ می‌کنیم و ادامه می‌دهیم.
            _logger.LogError(ex, "DeleteBookingFile: Error deleting physical file '{FilePath}' for FileId: {FileId}. Record will still be removed from DB.",
                fileToDelete.FilePathOrContentIdentifier, request.FileId);
            // بسته به سیاست برنامه، ممکن است بخواهید اینجا یک Exception پرتاب کنید و عملیات را متوقف کنید.
        }

        // ۵. حذف رکورد فایل از پایگاه داده
        await _unitOfWork.BookingFileRepository.DeleteAsync(fileToDelete);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("DeleteBookingFile: File record with ID {FileId} deleted successfully from DB for BookingRequest {BookingRequestId}.", 
            request.FileId, request.BookingRequestId);
    }
}