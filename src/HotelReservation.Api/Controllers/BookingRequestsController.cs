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
using HotelReservation.Application.DTOs.Common; // برای PagedResult

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
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "خطایی در پردازش درخواست رزرو رخ داد."});
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
        // این بخش نیاز به بازبینی دارد تا از ICurrentUserService استفاده کند
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
        
        var details = await _mediator.Send(query);
        
        if (details == null)
        {
            return NotFound();
        }

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
}
