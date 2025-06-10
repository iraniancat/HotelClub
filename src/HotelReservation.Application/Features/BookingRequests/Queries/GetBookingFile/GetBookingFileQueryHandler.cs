namespace HotelReservation.Application.Features.BookingRequests.Queries.GetBookingFile;

using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Infrastructure;
using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.DTOs.Booking;
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class GetBookingFileQueryHandler : IRequestHandler<GetBookingFileQuery, BookingFileDownloadDto>
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
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public async Task<BookingFileDownloadDto> Handle(GetBookingFileQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting to retrieve file with ID: {FileId} for BookingRequestId: {BookingRequestId} by User: {UserId}",
            request.FileId, request.BookingRequestId, _currentUserService.UserId);

        var bookingRequest = await _unitOfWork.BookingRequestRepository.GetByIdAsync(request.BookingRequestId, asNoTracking: true);
        if (bookingRequest == null)
        {
            throw new NotFoundException(nameof(BookingRequest), request.BookingRequestId);
        }

        var userPrincipal = _currentUserService.GetUserPrincipal();
        if (userPrincipal == null) throw new UnauthorizedAccessException();

        var authorizationResult = await _authorizationService.AuthorizeAsync(userPrincipal, bookingRequest, "CanViewBookingRequest");
        if (!authorizationResult.Succeeded)
        {
            throw new ForbiddenAccessException("شما مجاز به دسترسی به فایل‌های این درخواست رزرو نیستید.");
        }

        var bookingFileEntity = await _unitOfWork.BookingFileRepository.GetByIdAsync(request.FileId);
        if (bookingFileEntity == null || bookingFileEntity.BookingRequestId != request.BookingRequestId)
        {
            throw new NotFoundException($"فایل با شناسه '{request.FileId}' برای این درخواست رزرو یافت نشد.", nameof(BookingFile));
        }

        var fileContent = await _fileStorageService.GetFileAsync(bookingFileEntity.FilePathOrContentIdentifier);
        if (fileContent == null)
        {
            _logger.LogError("File content not found in storage for FilePath: {FilePath}, FileId: {FileId}",
                bookingFileEntity.FilePathOrContentIdentifier, request.FileId);
            throw new NotFoundException($"محتوای فایل '{bookingFileEntity.FileName}' در محل ذخیره‌سازی یافت نشد.", nameof(BookingFile));
        }

        _logger.LogInformation("File '{FileName}' (ID: {FileId}) retrieved successfully for download.",
            bookingFileEntity.FileName, request.FileId);

        return new BookingFileDownloadDto(fileContent, bookingFileEntity.ContentType, bookingFileEntity.FileName);
    }
}