// مسیر: src/HotelReservation.Api/Controllers/BookingRequestsController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using HotelReservation.Application.DTOs.Booking;
using HotelReservation.Application.Features.BookingRequests.Commands.CreateBookingRequest;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using HotelReservation.Application.Exceptions;
using FluentValidation;
using HotelReservation.Application.Features.BookingRequests.Queries.GetBookingRequestDetails;
using HotelReservation.Application.Contracts.Security; // برای CustomClaimTypes
using HotelReservation.Application.Features.BookingRequests.Queries.GetAllBookingRequests; // برای PagedResult
using HotelReservation.Application.DTOs.Common;
using HotelReservation.Application.Features.BookingRequests.Queries.GetBookingFile;
using HotelReservation.Application.Features.BookingRequests.Commands.CancelBookingRequest; // برای PagedResult
using HotelReservation.Application.Features.BookingRequests.Commands.ApproveBookingRequest;
using HotelReservation.Application.Features.BookingRequests.Commands.RejectBookingRequest;

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/booking-requests")]
[Authorize]
public class BookingRequestsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BookingRequestsController> _logger;

    public BookingRequestsController(IMediator mediator, ILogger<BookingRequestsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,ProvinceUser")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateBookingRequestResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // <<-- امضای متد اصلاح شده است. پارامتر DTO حذف شده است. -->>
    public async Task<IActionResult> CreateBookingRequest()
    {
        // داده‌ها و فایل را مستقیماً از فرم درخواست می‌خوانیم.
        var bookingDataJson = HttpContext.Request.Form["bookingData"].FirstOrDefault();
        var file = HttpContext.Request.Form.Files.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(bookingDataJson))
        {
            return BadRequest(new { message = "اطلاعات درخواست رزرو (bookingData) ارسال نشده است." });
        }

        CreateBookingRequestDto? actualDto;
        try
        {
            actualDto = JsonSerializer.Deserialize<CreateBookingRequestDto>(bookingDataJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (actualDto == null) throw new JsonException("Deserialized DTO is null.");
            // بررسی می‌کنیم که تاریخ‌ها مقدار داشته باشند
            if (!actualDto.CheckInDate.HasValue || !actualDto.CheckOutDate.HasValue)
            {
                return BadRequest(new { message = "تاریخ ورود و خروج الزامی است." });
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize bookingData JSON string: {JsonString}", bookingDataJson);
            return BadRequest(new { message = "فرمت داده‌های ارسالی درخواست رزرو نامعتبر است." });
        }

        // Handler مسئول خواندن شناسه کاربر از ICurrentUserService است.
        var createBookingCommand = new CreateBookingRequestCommand(
            actualDto.RequestingEmployeeNationalCode,
            actualDto.BookingPeriodId,
            actualDto.CheckInDate.Value,
            actualDto.CheckOutDate.Value,
            actualDto.HotelId,
            actualDto.Guests,
            actualDto.Notes
        );

        var createResponse = await _mediator.Send(createBookingCommand);

        if (createResponse == null)
        {
            _logger.LogError("CreateBookingRequestCommandHandler returned a null response for DTO: {BookingDto}", bookingDataJson);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "خطایی در پردازش درخواست رزرو رخ داد." });
        }

        // اگر فایل وجود داشت و رزرو با موفقیت ایجاد شد، فایل را به آن پیوست می‌کنیم
        if (file != null && createResponse.Id != Guid.Empty)
        {
            var submitterUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(submitterUserIdString, out Guid submitterUserId))
            {
                _logger.LogWarning("File upload for booking {BookingId} skipped: could not retrieve a valid SubmitterUserId from claims.", createResponse.Id);
            }
            else
            {
                try
                {
                    await using var fileStream = file.OpenReadStream();
                    var uploadFileCommand = new HotelReservation.Application.Features.BookingRequests.Commands.UploadBookingFile.UploadBookingFileCommand(
                        createResponse.Id,
                        fileStream,
                        file.FileName,
                        file.ContentType,
                        submitterUserId
                    );
                    await _mediator.Send(uploadFileCommand);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Booking request {BookingId} was created, but failed to attach the file '{FileName}'.", createResponse.Id, file.FileName);
                    createResponse.Message = $"درخواست رزرو با موفقیت ثبت شد، اما در پیوست کردن فایل خطایی رخ داد: {ex.Message}";
                }
            }
        }

        return CreatedAtAction(nameof(GetBookingRequestById), new { id = createResponse.Id }, createResponse);
    }

    [HttpGet("{id:guid}", Name = "GetBookingRequestById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BookingRequestDetailsDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Authorize]
    public async Task<IActionResult> GetBookingRequestById(Guid id)
    {
        // دیگر نیازی به خواندن Claimها در اینجا نیست
        var query = new GetBookingRequestDetailsQuery(id);

        // NotFoundException و ForbiddenAccessException توسط Middleware مدیریت می‌شوند
        var details = await _mediator.Send(query);

        return Ok(details);
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,ProvinceUser,HotelUser,Employee")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResult<BookingRequestSummaryDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllBookingRequests([FromQuery] string? statusFilter = null, [FromQuery] string? searchTerm = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
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
    [HttpGet("{bookingRequestId:guid}/files/{fileId:guid}/download", Name = "DownloadBookingFile")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize] // مجوز اولیه، جزئیات دقیق‌تر در Handler بررسی می‌شود
    public async Task<IActionResult> DownloadBookingFile(Guid bookingRequestId, Guid fileId)
    {
        if (bookingRequestId == Guid.Empty || fileId == Guid.Empty)
        {
            return BadRequest(new { message = "شناسه درخواست رزرو و شناسه فایل الزامی است." });
        }

        var query = new GetBookingFileQuery(bookingRequestId, fileId);

        // NotFoundException و ForbiddenAccessException توسط Middleware مدیریت می‌شوند
        var fileToDownload = await _mediator.Send(query);

        // بازگرداندن فایل به کلاینت
        // این متد به طور خودکار هدرهای لازم برای دانلود را تنظیم می‌کند.
        return File(fileToDownload.FileContent, fileToDownload.ContentType, fileToDownload.FileName);
    }
    [HttpPut("{id:guid}/cancel")]
    [Authorize(Roles = "SuperAdmin,ProvinceUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelBookingRequest(Guid id, [FromBody] CancelBookingRequestDto dto)
    {
        var command = new CancelBookingRequestCommand(id, dto.CancellationReason);
        await _mediator.Send(command);
        return NoContent();
    }
     [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "HotelUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ApproveBookingRequest(Guid id, [FromBody] ApproveBookingRequestDto dto)
    {
        var command = new ApproveBookingRequestCommand(id, dto.Comments);
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "HotelUser")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RejectBookingRequest(Guid id, [FromBody] RejectBookingRequestDto dto)
    {
        var command = new RejectBookingRequestCommand(id, dto.RejectionReason);
        await _mediator.Send(command);
        return NoContent();
    }
}
