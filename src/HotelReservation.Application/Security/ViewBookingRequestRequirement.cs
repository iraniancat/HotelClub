// src/HotelReservation.Application/Security/ViewBookingRequestRequirement.cs
using Microsoft.AspNetCore.Authorization; // برای IAuthorizationRequirement

namespace HotelReservation.Application.Security;

public class ViewBookingRequestRequirement : IAuthorizationRequirement
{
    public ViewBookingRequestRequirement() { }
}