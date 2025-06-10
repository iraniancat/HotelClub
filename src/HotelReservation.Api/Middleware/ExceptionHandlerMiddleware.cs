// src/HotelReservation.Api/Middleware/ExceptionHandlerMiddleware.cs
using HotelReservation.Application.Exceptions; // برای NotFoundException و سایر Exceptionهای سفارشی Application
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging; // برای لاگ کردن خطاها
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json; // برای سریالایز کردن به JSON
using System.Threading.Tasks;
using FluentValidation;
using System.Text.Encodings.Web; // برای ValidationException

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
        // <<-- اصلاح کلیدی: تنظیم Content-Type با charset=utf-8 -->>
        context.Response.ContentType = "application/json; charset=utf-8";
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
        object? responsePayload = null;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                responsePayload = new { title = "خطای اعتبارسنجی", status = (int)statusCode, errors = validationException.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }) };
                break;
            case NotFoundException notFoundException:
                statusCode = HttpStatusCode.NotFound;
                responsePayload = new { title = "یافت نشد", status = (int)statusCode, detail = notFoundException.Message };
                break;
            case BadRequestException badRequestException:
                statusCode = HttpStatusCode.BadRequest;
                responsePayload = new { title = "درخواست نامعتبر", status = (int)statusCode, detail = badRequestException.Message };
                break;
            case ForbiddenAccessException forbiddenAccessException:
                statusCode = HttpStatusCode.Forbidden;
                responsePayload = new { title = "دسترسی غیرمجاز", status = (int)statusCode, detail = forbiddenAccessException.Message };
                break;
            case UnauthorizedAccessException unauthorizedAccessException:
                 statusCode = HttpStatusCode.Unauthorized;
                responsePayload = new { title = "احراز هویت ناموفق", status = (int)statusCode, detail = unauthorizedAccessException.Message };
                break;
            default:
                _logger.LogError(exception, "An unhandled exception has occurred.");
                responsePayload = new { title = "خطای داخلی سرور", status = (int)statusCode, detail = "یک خطای پیش‌بینی نشده در سرور رخ داده است." };
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        
        // اطمینان از اینکه خروجی JSON با انکودینگ صحیح (Unicode) نوشته می‌شود
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // <<-- این خط از escape کردن کاراکترهای فارسی جلوگیری می‌کند
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(responsePayload, options));
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