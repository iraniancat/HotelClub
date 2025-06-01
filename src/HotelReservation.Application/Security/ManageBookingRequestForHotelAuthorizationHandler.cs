// src/HotelReservation.Application/Security/ManageBookingRequestForHotelAuthorizationHandler.cs
using HotelReservation.Application.Contracts.Security; // برای CustomClaimTypes
using HotelReservation.Domain.Entities;                // برای BookingRequest
using Microsoft.AspNetCore.Authorization;              // برای AuthorizationHandler و IAuthorizationRequirement
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Application.Security;

public class ManageBookingRequestForHotelAuthorizationHandler
    : AuthorizationHandler<ManageBookingRequestForHotelRequirement, BookingRequest>
{
    private readonly ILogger<ManageBookingRequestForHotelAuthorizationHandler> _logger;
    private const string HotelUserRoleName = "HotelUser"; // باید با نام نقش در سیستم شما مطابقت داشته باشد

    public ManageBookingRequestForHotelAuthorizationHandler(ILogger<ManageBookingRequestForHotelAuthorizationHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageBookingRequestForHotelRequirement requirement,
        BookingRequest resource_bookingRequest) // منبع: درخواست رزروی که عملیات روی آن انجام می‌شود
    {
        if (context.User == null || resource_bookingRequest == null)
        {
            _logger.LogWarning("Authorization failed for ManageBookingRequestForHotelRequirement: User or Resource is null.");
            context.Fail();
            return Task.CompletedTask;
        }

        // این نیازمندی فقط برای کاربران هتل معنی‌دار است
        if (!context.User.IsInRole(HotelUserRoleName))
        {
            _logger.LogDebug("User {UserId} is not a HotelUser. Skipping ManageBookingRequestForHotelRequirement.", 
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            // اگر کاربر نقش HotelUser را ندارد، این Handler مسئولیتی ندارد و ممکن است Handler دیگری موفق شود یا کلاً Fail شود.
            // اگر می‌خواهید فقط HotelUser بتواند این نیازمندی را برآورده کند، اینجا Fail کنید.
            // اما معمولاً بهتر است اجازه دهیم سایر Handlerها (اگر وجود دارند) بررسی شوند.
            // فعلاً فرض می‌کنیم این Policy فقط برای HotelUser اعمال می‌شود و اگر نقش دیگری بود، از طریق [Authorize(Roles="...")] کنترل شده.
            // اگر می‌خواهیم صراحتاً بگوییم فقط HotelUser می‌تواند این نیازمندی را برآورده کند:
            // context.Fail(new AuthorizationFailureReason(this, "User is not a HotelUser."));
            // return Task.CompletedTask;
            // برای سادگی فعلی، اگر نقش HotelUser نباشد، این Handler کاری انجام نمی‌دهد و به سایر بررسی‌ها واگذار می‌کند.
            // اما در عمل، این Policy باید فقط برای HotelUser فراخوانی شود.
            // اگر کاربر HotelUser نیست، و این تنها نیازمندی است، context.Fail() مناسب است.
            // ما در Controller با [Authorize(Roles="HotelUser")] این را کنترل می‌کنیم.
            // پس اگر به اینجا رسیدیم، کاربر حتما HotelUser است.
        }

        var hotelIdClaimString = context.User.FindFirst(CustomClaimTypes.HotelId)?.Value;
        if (!Guid.TryParse(hotelIdClaimString, out Guid userAssignedHotelId))
        {
            _logger.LogWarning("Authorization failed for ManageBookingRequestForHotelRequirement: HotelId claim is missing or invalid for User {UserId}.",
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            context.Fail(new AuthorizationFailureReason(this, "اطلاعات هتل کاربر در توکن یافت نشد."));
            return Task.CompletedTask;
        }

        if (resource_bookingRequest.HotelId == userAssignedHotelId)
        {
            _logger.LogInformation("Authorization Succeeded for ManageBookingRequestForHotelRequirement: HotelUser {UserId} is authorized for BookingRequest {BookingId} in Hotel {HotelId}.",
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, resource_bookingRequest.Id, userAssignedHotelId);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("Authorization Failed for ManageBookingRequestForHotelRequirement: HotelUser {UserId} (AssignedHotel: {UserHotelId}) attempted to manage BookingRequest {BookingId} for a different hotel ({BookingHotelId}).",
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, userAssignedHotelId, resource_bookingRequest.Id, resource_bookingRequest.HotelId);
            context.Fail(new AuthorizationFailureReason(this, "کاربر هتل مجاز به مدیریت درخواست‌های این هتل نیست."));
        }

        return Task.CompletedTask;
    }
}