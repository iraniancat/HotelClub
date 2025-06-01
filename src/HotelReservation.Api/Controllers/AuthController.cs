// src/HotelReservation.Api/Controllers/AuthController.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HotelReservation.Application.DTOs.Authentication; // برای LoginRequestDto و LoginResponseDto
using HotelReservation.Application.Features.Authentication.Queries.LoginUser; // برای LoginUserQuery
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization; // برای StatusCodes
// using HotelReservation.Application.Exceptions; // اگر می‌خواهید try-catch خاصی در کنترلر داشته باشید

namespace HotelReservation.Api.Controllers;

[ApiController]
[Route("api/auth")] // مسیر پایه: api/auth
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger; // اضافه کردن لاگر

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("login")]
    [AllowAnonymous] 
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] // برای خطاهای اعتبارسنجی یا نام کاربری/رمز عبور نامعتبر
    // [ProducesResponseType(StatusCodes.Status401Unauthorized)] // اگر بخواهیم به طور خاص برای خطای احراز هویت استفاده کنیم
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
    {
        if (loginRequestDto == null)
        {
            return BadRequest(new { message = "اطلاعات ورود ارسال نشده است." });
        }

        var query = new LoginUserQuery(loginRequestDto.SystemUserId, loginRequestDto.Password);
        
        // در اینجا، نیازی به بلوک try-catch برای BadRequestException نیست،
        // زیرا Global Exception Handling Middleware ما آن را گرفته و به پاسخ 400 تبدیل می‌کند.
        // اگر ValidationBehavior هم خطایی پرتاب کند (ValidationException)، آن هم توسط Middleware به 400 تبدیل می‌شود.
        var loginResponse = await _mediator.Send(query);

        // اگر Handler در صورت عدم موفقیت احراز هویت null برگرداند (به جای پرتاب Exception)،
        // باید اینجا null را چک کرده و Unauthorized یا BadRequest برگردانیم.
        // اما طراحی فعلی ما این است که Handler در صورت خطا، Exception پرتاب می‌کند.
        // if (loginResponse == null)
        // {
        //     return Unauthorized(new { message = "نام کاربری یا رمز عبور نامعتبر است." });
        // }

        _logger.LogInformation("User {SystemUserId} logged in successfully.", loginResponse.SystemUserId);
        return Ok(loginResponse);
    }
}