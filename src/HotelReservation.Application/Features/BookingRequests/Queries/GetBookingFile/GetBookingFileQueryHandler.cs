// src/HotelReservation.Application/Features/BookingRequests/Queries/GetBookingFile/GetBookingFileQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Infrastructure; // برای IFileStorageService
using HotelReservation.Application.Contracts.Security;    // برای ICurrentUserService و CustomClaimTypes
using HotelReservation.Application.DTOs.Booking;          // برای BookingFileDownloadDto
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization; // برای IAuthorizationService
using Microsoft.Extensions.Logging;
using System;
using System.IO; // برای Path
using System.Linq; // برای FirstOrDefault
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic; // برای List<Claim>

namespace HotelReservation.Application.Features.BookingRequests.Queries.GetBookingFile;

public class GetBookingFileQueryHandler : IRequestHandler<GetBookingFileQuery, BookingFileDownloadDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<GetBookingFileQueryHandler> _logger;

    public GetBookingFileQueryHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        ILogger<GetBookingFileQueryHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BookingFileDownloadDto?> Handle(GetBookingFileQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting to retrieve file with ID: {FileId} for BookingRequestId: {BookingRequestId} by User: {UserId}",
            request.FileId, request.BookingRequestId, _currentUserService.UserId);

        // ۱. واکشی درخواست رزرو به همراه فایل‌هایش (یا فقط فایل مورد نظر)
        var bookingRequest = await _unitOfWork.BookingRequestRepository.GetBookingRequestWithDetailsAsync(request.BookingRequestId);
        if (bookingRequest == null)
        {
            _logger.LogWarning("GetBookingFile: BookingRequest with ID {BookingRequestId} not found.", request.BookingRequestId);
            throw new NotFoundException(nameof(BookingRequest), request.BookingRequestId);
        }

        // ۲. بررسی مجوز کاربر برای دسترسی به این درخواست رزرو (و در نتیجه فایل‌های آن)
        var userPrincipal = _currentUserService.GetUserPrincipal();
        if (userPrincipal == null)
        {
            _logger.LogWarning("GetBookingFile: User is not authenticated. BookingRequestId: {BookingRequestId}", request.BookingRequestId);
            throw new UnauthorizedAccessException("کاربر برای دسترسی به این فایل احراز هویت نشده است.");
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(
            userPrincipal,
            bookingRequest, // منبع: خود درخواست رزرو
            "CanViewBookingRequest" // استفاده از همان Policy مشاهده جزئیات رزرو
        );

        if (!authorizationResult.Succeeded)
        {
            _logger.LogWarning(
                "GetBookingFile: User {UserId} failed authorization policy 'CanViewBookingRequest' for BookingRequest {BookingRequestId}.",
                _currentUserService.UserId, request.BookingRequestId);
            throw new ForbiddenAccessException("شما مجاز به دسترسی به فایل‌های این درخواست رزرو نیستید.");
        }
        _logger.LogInformation("GetBookingFile: User {UserId} authorized for BookingRequest {BookingRequestId}.", 
            _currentUserService.UserId, request.BookingRequestId);


        // ۳. یافتن اطلاعات فایل از لیست فایل‌های درخواست رزرو
        var bookingFileEntity = await _unitOfWork.BookingFileRepository.GetByIdAsync(request.FileId);
        // var bookingFileEntity = bookingRequest.Files.FirstOrDefault(f => f.Id == request.FileId); // اگر Files در GetBookingRequestWithDetailsAsync لود شده باشد

        if (bookingFileEntity == null || bookingFileEntity.BookingRequestId != request.BookingRequestId)
        {
            _logger.LogWarning("GetBookingFile: File with ID {FileId} not found or does not belong to BookingRequest {BookingRequestId}.", 
                request.FileId, request.BookingRequestId);
            throw new NotFoundException($"فایل با شناسه '{request.FileId}' برای این درخواست رزرو یافت نشد.", nameof(BookingFile));
        }

        // ۴. خواندن محتوای فایل از سرویس ذخیره‌سازی
        var fileContent = await _fileStorageService.GetFileAsync(bookingFileEntity.FilePathOrContentIdentifier);
        if (fileContent == null)
        {
            _logger.LogError("GetBookingFile: File content not found in storage for FilePath: {FilePath}, FileId: {FileId}", 
                bookingFileEntity.FilePathOrContentIdentifier, request.FileId);
            throw new NotFoundException($"محتوای فایل '{bookingFileEntity.FileName}' در محل ذخیره‌سازی یافت نشد.", nameof(BookingFile));
        }

        _logger.LogInformation("GetBookingFile: File '{FileName}' (ID: {FileId}) retrieved successfully for BookingRequest {BookingRequestId}.",
            bookingFileEntity.FileName, request.FileId, request.BookingRequestId);

        return new BookingFileDownloadDto(fileContent, bookingFileEntity.ContentType, bookingFileEntity.FileName);
    }
}