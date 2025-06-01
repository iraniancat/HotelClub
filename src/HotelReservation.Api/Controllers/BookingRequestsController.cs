// src/HotelReservation.Api/Controllers/BookingRequestsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using HotelReservation.Application.DTOs.Booking; // برای CreateBookingRequestDto و CreateBookingRequestResponseDto
using HotelReservation.Application.Features.BookingRequests.Commands.CreateBookingRequest; // برای CreateBookingRequestCommand
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using HotelReservation.Application.Exceptions;
using HotelReservation.Application.Features.BookingRequests.Queries.GetBookingRequestDetails;
using HotelReservation.Application.Features.BookingRequests.Queries.GetAllBookingRequests;
using HotelReservation.Application.Features.BookingRequests.Commands.ApproveBookingRequest;
using HotelReservation.Application.Features.BookingRequests.Commands.RejectBookingRequest;
using Microsoft.AspNetCore.Authorization;
using HotelReservation.Application.Contracts.Security;
using HotelReservation.Application.DTOs.Common;
using HotelReservation.Application.Features.BookingRequests.Commands.UploadBookingFile;
using HotelReservation.Application.Features.BookingRequests.Queries.GetBookingFile; // برای خواندن اطلاعات کاربر احراز هویت شده

namespace HotelReservation.Api.Controllers;

[ApiController]
[Authorize] // <<-- تمام Actionهای این Controller به طور پیش‌فرض نیاز به احراز هویت دارند
[Route("api/booking-requests")]
// TODO (Authorization): [Authorize(Roles = "SuperAdmin,ProvinceUser")] - این کنترلر باید فقط برای این نقش‌ها در دسترس باشد
public class BookingRequestsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BookingRequestsController> _logger; // اضافه کردن لاگر برای عیب‌یابی

    public BookingRequestsController(IMediator mediator, ILogger<BookingRequestsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,ProvinceUser")] // <<-- نقش‌های مجاز برای ایجاد
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateBookingRequestResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateBookingRequest([FromBody] CreateBookingRequestDto dto)
    {
        // ... (بخش دریافت submitterUserId همانند قبل) ...
        var submitterUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid submitterUserId;
        if (string.IsNullOrEmpty(submitterUserIdString) || !Guid.TryParse(submitterUserIdString, out submitterUserId))
        {
            _logger.LogWarning("Submitter User ID not found in claims or is invalid for CreateBookingRequest.");
            return Unauthorized(new { message = "کاربر احراز هویت نشده یا شناسه کاربر معتبر نیست." });
        }

        var command = new CreateBookingRequestCommand(
            dto.RequestingEmployeeNationalCode,
            dto.BookingPeriod,
            dto.CheckInDate,
            dto.CheckOutDate,
            dto.HotelId,
            dto.Guests,
            submitterUserId,
            dto.Notes
        );

        try
        {
            var response = await _mediator.Send(command);
            // استفاده از CreatedAtAction با ارجاع به GetBookingRequestById
            return CreatedAtAction(nameof(GetBookingRequestById), new { id = response.Id }, response);
        }
        // ... (بلوک‌های catch همانند قبل) ...
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error during booking request creation.");
            var errors = ex.Errors.Select(e => new
            {
                Property = e.PropertyName,
                Message = e.ErrorMessage,
                AttemptedValue = e.AttemptedValue
            });
            return BadRequest(new { title = "خطای اعتبارسنجی.", status = StatusCodes.Status400BadRequest, errors });
        }
        catch (NotFoundException ex)
        {
            _logger.LogInformation(ex, "A related entity was not found during booking request creation.");
            return NotFound(new { title = "منبع یافت نشد.", status = StatusCodes.Status404NotFound, detail = ex.Message });
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning(ex, "Bad request error during booking request creation.");
            return BadRequest(new { title = "درخواست نامعتبر.", status = StatusCodes.Status400BadRequest, detail = ex.Message });
        }
    }

    [HttpGet("{id:guid}", Name = "GetBookingRequestById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingRequestDetailsDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]    // برای NotFoundException
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]    // برای ForbiddenAccessException
    [Authorize] // <<-- اطمینان از احراز هویت اولیه
    public async Task<IActionResult> GetBookingRequestById(Guid id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid authenticatedUserId) || string.IsNullOrEmpty(userRole))
        {
            _logger.LogWarning("GetBookingRequestById (ID: {BookingId}): User ID or Role claim not found or invalid in token.", id);
            return Unauthorized(new { message = "اطلاعات احراز هویت کاربر ناقص یا نامعتبر است." });
        }

        string? provinceCodeClaim = User.FindFirstValue(CustomClaimTypes.ProvinceCode);
        Guid? hotelIdClaim = null;
        var hotelIdString = User.FindFirstValue(CustomClaimTypes.HotelId);
        if (!string.IsNullOrEmpty(hotelIdString) && Guid.TryParse(hotelIdString, out Guid parsedHotelId))
        {
            hotelIdClaim = parsedHotelId;
        }

        var query = new GetBookingRequestDetailsQuery(
            id,
            authenticatedUserId,
            userRole,
            provinceCodeClaim,
            hotelIdClaim
        );

        // NotFoundException و ForbiddenAccessException توسط Middleware مدیریت می‌شوند
        // پس نیازی به بررسی null بودن نتیجه در اینجا نیست اگر Handler آن‌ها را پرتاب کند.
        var bookingRequestDetails = await _mediator.Send(query);

        return Ok(bookingRequestDetails); // اگر به اینجا برسد یعنی موفقیت‌آمیز بوده
    }
    [HttpGet]
    [Authorize] // <<-- نقش‌های مجاز برای مشاهده جزئیات
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BookingRequestSummaryDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<BookingRequestSummaryDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize(Roles = "SuperAdmin,ProvinceUser,HotelUser,Employee")]
    public async Task<IActionResult> GetAllBookingRequests(
        [FromQuery] string? statusFilter = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // <<-- حذف بخش خواندن Claimها از User -->>
        // اطلاعات کاربر احراز هویت شده از طریق ICurrentUserService در Handler در دسترس خواهد بود.

        var query = new GetAllBookingRequestsQuery
        {
            StatusFilter = statusFilter,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
    [HttpPut("{id:guid}/approve")] // یا HttpPost    
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "HotelUser")] // اطمینان از اینکه کاربر نقش HotelUser را دارد
    public async Task<IActionResult> ApproveBookingRequest(Guid id, [FromBody] ApproveBookingRequestDto? dto)
    {
        var hotelUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(hotelUserIdString) || !Guid.TryParse(hotelUserIdString, out Guid hotelUserId))
        {
            // ... (همانند قبل) ...
            _logger.LogWarning($"ApproveBookingRequest: Hotel User ID (NameIdentifier claim) not found or invalid for approving booking {id}.");
            return Unauthorized(new { message = "کاربر هتل احراز هویت نشده یا شناسه کاربر معتبر نیست." });
        }
        var command = new ApproveBookingRequestCommand(id, hotelUserId, dto?.Comments);
        await _mediator.Send(command); // NotFoundException یا ForbiddenAccessException توسط Middleware مدیریت می‌شود
        return NoContent();
    }
    [HttpPut("{id:guid}/reject")] // یا HttpPost
    [Authorize(Roles = "HotelUser")] // <<-- فقط کاربر هتل
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectBookingRequest(Guid id, [FromBody] RejectBookingRequestDto dto)
    {
        // ... (همانند قبل) ...
        if (dto == null)
        {
            return BadRequest(new { message = "اطلاعات لازم برای رد درخواست (دلیل رد) ارسال نشده است." });
        }
        var hotelUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(hotelUserIdString) || !Guid.TryParse(hotelUserIdString, out Guid hotelUserId))
        {
            _logger.LogWarning($"RejectBookingRequest: Hotel User ID (NameIdentifier claim) not found or invalid for rejecting booking {id}.");
            return Unauthorized(new { message = "کاربر هتل احراز هویت نشده یا شناسه کاربر معتبر نیست." });
        }
        var command = new RejectBookingRequestCommand(id, hotelUserId, dto.RejectionReason);
        await _mediator.Send(command);
        return NoContent();
    }
    [HttpPost("{bookingRequestId:guid}/files")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BookingFileDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    // TODO (Authorization): نقش‌های مجاز را دقیق‌تر مشخص کنید، مثلاً کاربر ثبت کننده درخواست یا مدیران
    // [Authorize(Roles = "SuperAdmin,ProvinceUser")] // یا بر اساس Policy خاص
    [Authorize] // فعلاً هر کاربر احراز هویت شده (بعداً با Policy دقیق‌تر می‌شود)
    public async Task<IActionResult> UploadBookingFile(Guid bookingRequestId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "فایلی برای آپلود انتخاب نشده است." });
        }

        // بررسی اولیه نوع و اندازه فایل در Controller نیز می‌تواند انجام شود،
        // اگرچه Validator ما در Command هم این کار را انجام می‌دهد.
        // این کار از ارسال داده‌های بزرگ و نامعتبر به لایه Application جلوگیری می‌کند.
        // مثال:
        // const long maxFileSize = 5 * 1024 * 1024; // 5 MB
        // if (file.Length > maxFileSize)
        // {
        //     return BadRequest(new { message = $"حداکثر اندازه مجاز برای فایل {maxFileSize / 1024 / 1024} مگابایت است." });
        // }
        // var allowedContentTypes = new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };
        // if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
        // {
        //     return BadRequest(new { message = "نوع فایل مجاز نیست. فقط فایل‌های Word و PDF پشتیبانی می‌شوند."});
        // }


        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid uploadedByUserId))
        {
            _logger.LogWarning("UploadBookingFile: User ID (NameIdentifier claim) not found or invalid for uploading file to BookingRequest {BookingRequestId}.", bookingRequestId);
            return Unauthorized(new { message = "کاربر برای آپلود فایل احراز هویت نشده یا شناسه کاربر معتبر نیست." });
        }

        // از IFormFile.OpenReadStream() برای گرفتن استریم استفاده می‌کنیم.
        // مهم است که این استریم پس از استفاده Dispose شود. Handler این کار را انجام نمی‌دهد،
        // بنابراین یا باید اینجا Dispose شود یا بایت‌های آن خوانده و به Command پاس داده شود.
        // برای سادگی فعلی، استریم را پاس می‌دهیم و فرض می‌کنیم FileSystemStorageService آن را مدیریت می‌کند.
        // (پیاده‌سازی FileSystemStorageService ما از using برای FileStream استفاده می‌کند که Dispose را تضمین می‌کند)

        await using var fileStream = file.OpenReadStream(); // <<-- مهم: استفاده از await using برای Dispose شدن استریم

        var command = new UploadBookingFileCommand(
            bookingRequestId,
            fileStream, // استریم فایل
            file.FileName,  // نام اصلی فایل از کلاینت
            file.ContentType, // نوع محتوای فایل از کلاینت
            uploadedByUserId
        );

        try
        {
            var bookingFileDto = await _mediator.Send(command);

            // یک URL برای دسترسی به فایل ایجاد شده (اگر Endpoint دانلود داشته باشیم)
            // فعلاً BookingFileDto شامل یک URL فرضی است.
            // می‌توانیم پاسخ CreatedAtAction را به Endpoint دانلود فایل (که بعداً ایجاد می‌کنیم) بدهیم.
            _logger.LogInformation("File '{FileName}' uploaded successfully for BookingRequest {BookingRequestId} by User {UserId}. File ID: {FileId}",
                command.FileName, command.BookingRequestId, command.UploadedByUserId, bookingFileDto.Id);

            return StatusCode(StatusCodes.Status201Created, bookingFileDto);
        }
        // Exception های خاص مانند ValidationException, NotFoundException, ForbiddenAccessException, BadRequestException
        // توسط Global Exception Handling Middleware مدیریت می‌شوند.
        // نیازی به try-catch های مجزا در اینجا نیست مگر برای لاگ کردن یا رفتار خاص.
        catch (Exception ex) // یک catch عمومی برای لاگ کردن خطاهای پیش‌بینی نشده در سطح Controller
        {
            _logger.LogError(ex, "Error during UploadBookingFile for BookingRequestId {BookingRequestId} by User {UserId}",
                bookingRequestId, uploadedByUserId);
            // Middleware بقیه کار را انجام می‌دهد و پاسخ مناسب (احتمالاً 500) را برمی‌گرداند.
            throw; // دوباره پرتاب خطا تا Middleware آن را بگیرد.
        }
    }
     [HttpGet("{bookingRequestId:guid}/files/{fileId:guid}/download", Name = "DownloadBookingFile")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)] // نوع پاسخ فایل است
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // برای ورودی‌های نامعتبر (اگر Query پارامتر داشت)
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize] // مجوز اولیه، جزئیات توسط Handler و Policy بررسی می‌شود
    public async Task<IActionResult> DownloadBookingFile(Guid bookingRequestId, Guid fileId)
    {
        if (bookingRequestId == Guid.Empty || fileId == Guid.Empty)
        {
            return BadRequest(new { message = "شناسه درخواست رزرو و شناسه فایل الزامی و معتبر هستند." });
        }

        var query = new GetBookingFileQuery(bookingRequestId, fileId);
        
        // NotFoundException و ForbiddenAccessException توسط Middleware مدیریت می‌شوند
        var fileDownloadDto = await _mediator.Send(query);

        if (fileDownloadDto == null) 
        {
             // این حالت نباید رخ دهد اگر Handler به درستی Exception پرتاب کند
            _logger.LogWarning("DownloadBookingFile: FileDownloadDto was null for BookingRequest {BookingRequestId}, File {FileId} after handler execution without exception.",
                bookingRequestId, fileId);
            return NotFound(new { message = "فایل درخواستی یافت نشد یا خطایی در پردازش رخ داده است." });
        }

        // بازگرداندن فایل به کلاینت
        return File(fileDownloadDto.FileContent, fileDownloadDto.ContentType, fileDownloadDto.FileName);
    }
}