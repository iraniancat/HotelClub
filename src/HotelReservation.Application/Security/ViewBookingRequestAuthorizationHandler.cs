// src/HotelReservation.Application/Security/ViewBookingRequestAuthorizationHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Contracts.Security;
using HotelReservation.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging; // اضافه کردن لاگر
using System; // برای Guid.TryParse
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelReservation.Application.Security;

public class ViewBookingRequestAuthorizationHandler : AuthorizationHandler<ViewBookingRequestRequirement, BookingRequest>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ViewBookingRequestAuthorizationHandler> _logger;

    private const string SuperAdminRoleName = "SuperAdmin";
    private const string ProvinceUserRoleName = "ProvinceUser";
    private const string HotelUserRoleName = "HotelUser";
    private const string EmployeeRoleName = "Employee"; // برای فاز آتی

    public ViewBookingRequestAuthorizationHandler(IUnitOfWork unitOfWork, ILogger<ViewBookingRequestAuthorizationHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ViewBookingRequestRequirement requirement,
        BookingRequest resource_bookingRequest) // تغییر نام پارامتر برای وضوح بیشتر
    {
        if (context.User == null || resource_bookingRequest == null)
        {
            _logger.LogWarning("Authorization failed: User or Resource is null.");
            context.Fail();
            return;
        }

        var authenticatedUserIdString = context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        if (!Guid.TryParse(authenticatedUserIdString, out Guid authenticatedUserId))
        {
            _logger.LogWarning("Authorization failed: Could not parse AuthenticatedUserId from claims. Claim value: {ClaimValue}", authenticatedUserIdString);
            context.Fail();
            return;
        }

        var userRole = context.User.FindFirst(ClaimTypes.Role).Value;
        if (string.IsNullOrEmpty(userRole))
        {
            _logger.LogWarning("Authorization failed: UserRole claim is missing for AuthenticatedUserId: {UserId}", authenticatedUserId);
            context.Fail();
            return;
        }

        _logger.LogInformation("Authorizing User {UserId} with Role {UserRole} for viewing BookingRequest {BookingId}",
            authenticatedUserId, userRole, resource_bookingRequest.Id);

        // ۱. مدیر ارشد همیشه دسترسی دارد
        if (userRole == SuperAdminRoleName)
        {
            _logger.LogInformation("Authorization Succeeded for SuperAdmin {UserId} on BookingRequest {BookingId}", authenticatedUserId, resource_bookingRequest.Id);
            context.Succeed(requirement);
            return;
        }

        // ۲. کاربری که درخواست را ثبت کرده، دسترسی دارد
        if (resource_bookingRequest.RequestSubmitterUserId == authenticatedUserId)
        {
            _logger.LogInformation("Authorization Succeeded: User {UserId} is the submitter of BookingRequest {BookingId}", authenticatedUserId, resource_bookingRequest.Id);
            context.Succeed(requirement);
            return;
        }

        // ۳. کارمندی که درخواست برای او ثبت شده، دسترسی دارد
        var requestingEmployeeUser = await _unitOfWork.UserRepository.GetByNationalCodeAsync(resource_bookingRequest.RequestingEmployeeNationalCode);
        if (requestingEmployeeUser != null && requestingEmployeeUser.Id == authenticatedUserId)
        {
            _logger.LogInformation("Authorization Succeeded: User {UserId} is the main employee of BookingRequest {BookingId}", authenticatedUserId, resource_bookingRequest.Id);
            context.Succeed(requirement);
            return;
        }

        // ۴. کاربر هتل مربوطه دسترسی دارد
        if (userRole == HotelUserRoleName)
        {
            var hotelIdClaimString = context.User.FindFirst(CustomClaimTypes.HotelId).Value;
            if (Guid.TryParse(hotelIdClaimString, out Guid userHotelId) && resource_bookingRequest.HotelId == userHotelId)
            {
                _logger.LogInformation("Authorization Succeeded: HotelUser {UserId} is assigned to Hotel {HotelId} of BookingRequest {BookingId}", authenticatedUserId, userHotelId, resource_bookingRequest.Id);
                context.Succeed(requirement);
                return;
            }
             _logger.LogWarning("Authorization Failed for HotelUser {UserId}: HotelId claim mismatch or missing. UserHotelIdClaim: {UserHotelIdClaim}, BookingHotelId: {BookingHotelId}",
                authenticatedUserId, hotelIdClaimString, resource_bookingRequest.HotelId);
        }

        // ۵. کاربر استان مربوطه دسترسی دارد
        if (userRole == ProvinceUserRoleName)
        {
            var provinceCodeClaim = context.User.FindFirst(CustomClaimTypes.ProvinceCode).Value;
            // کارمند اصلی درخواست باید از استان کاربر استان باشد
            // (requestingEmployeeUser قبلاً واکشی شده)
            if (requestingEmployeeUser != null && !string.IsNullOrEmpty(provinceCodeClaim) && requestingEmployeeUser.ProvinceCode == provinceCodeClaim)
            {
                _logger.LogInformation("Authorization Succeeded: ProvinceUser {UserId} (Province: {UserProvince}) for BookingRequest {BookingId} of employee in same province (EmployeeProvince: {EmployeeProvince})",
                    authenticatedUserId, provinceCodeClaim, resource_bookingRequest.Id, requestingEmployeeUser.ProvinceCode);
                context.Succeed(requirement);
                return;
            }
            _logger.LogWarning("Authorization Failed for ProvinceUser {UserId}: ProvinceCode claim mismatch or employee not in same province. UserProvinceClaim: {UserProvinceClaim}, Employee (NC:{EmployeeNC}) Province: {EmployeeProvince}",
                authenticatedUserId, provinceCodeClaim, resource_bookingRequest.RequestingEmployeeNationalCode, requestingEmployeeUser?.ProvinceCode);
        }
        
        _logger.LogWarning("Authorization Failed for User {UserId} with Role {UserRole} on BookingRequest {BookingId}. No applicable rule met.",
            authenticatedUserId, userRole, resource_bookingRequest.Id);
        context.Fail();
    }
}