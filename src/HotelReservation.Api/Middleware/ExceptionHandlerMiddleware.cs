// src/HotelReservation.Api/Middleware/ExceptionHandlerMiddleware.cs
using HotelReservation.Application.Exceptions; // برای NotFoundException و سایر Exceptionهای سفارشی Application
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging; // برای لاگ کردن خطاها
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json; // برای سریالایز کردن به JSON
using System.Threading.Tasks;
using FluentValidation; // برای ValidationException

namespace HotelReservation.Api.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext); // اجرای Middleware بعدی در Pipeline یا خود Endpoint
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

     private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
        string title = "خطای داخلی سرور"; // عنوان پیش‌فرض
        string detail = "یک خطای پیش‌بینی نشده در سرور رخ داده است.";
        object? errors = null;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                title = "خطای اعتبارسنجی";
                detail = "یک یا چند خطای اعتبارسنجی رخ داده است.";
                errors = validationException.Errors.Select(e => new { 
                    property = e.PropertyName, // نام فیلد
                    message = e.ErrorMessage,
                    attemptedValue = e.AttemptedValue 
                });
                _logger.LogWarning(validationException, "Validation error occurred. Errors: {ValidationErrors}", JsonSerializer.Serialize(errors));
                break;

            case NotFoundException notFoundException:
                statusCode = HttpStatusCode.NotFound;
                title = "منبع یافت نشد";
                detail = notFoundException.Message;
                _logger.LogInformation(notFoundException, "Resource not found: {NotFoundMessage}", notFoundException.Message);
                break;

            case BadRequestException badRequestException:
                statusCode = HttpStatusCode.BadRequest;
                title = "درخواست نامعتبر";
                detail = badRequestException.Message;
                _logger.LogWarning(badRequestException, "Bad request error: {BadRequestMessage}", badRequestException.Message);
                break;
            
            case ForbiddenAccessException forbiddenAccessException: // <<-- اضافه شد
                statusCode = HttpStatusCode.Forbidden;
                title = "دسترسی غیرمجاز";
                detail = forbiddenAccessException.Message; // پیام Exception که "شما مجاز به انجام این عملیات نیستید." بود
                _logger.LogWarning(forbiddenAccessException, "Forbidden access attempt: {ForbiddenMessage}", forbiddenAccessException.Message);
                break;

            default: // سایر خطاهای پیش‌بینی نشده
                _logger.LogError(exception, "An unhandled exception has occurred: {ExceptionMessage}", exception.Message);
                // در محیط Production، جزئیات Exception اصلی نباید به کلاینت ارسال شود.
                // if (context.Request.Host.Host.Contains("localhost")) // یک راه برای تشخیص محیط توسعه (بهتر است از IWebHostEnvironment استفاده شود)
                // {
                //     detail = exception.ToString(); 
                // }
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        // استفاده از ساختار ProblemDetails برای پاسخ خطا (RFC 7807)
        var problemDetails = new 
        {
            title,
            status = (int)statusCode,
            detail,
            errors // فقط برای ValidationException مقدار دارد، برای بقیه null است
        };
        
        var jsonResponse = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull // فیلدهای null سریالایز نشوند
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
// متد افزونه برای راحتی در ثبت Middleware
public static class ExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}