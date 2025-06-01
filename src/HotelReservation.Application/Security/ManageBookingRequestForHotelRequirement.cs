// src/HotelReservation.Application/Security/ManageBookingRequestForHotelRequirement.cs
using Microsoft.AspNetCore.Authorization;

namespace HotelReservation.Application.Security;

public class ManageBookingRequestForHotelRequirement : IAuthorizationRequirement
{
    // این نیازمندی می‌تواند شامل پارامترهای خاص عملیات باشد،
    // مثلاً نوع عملیات (Approve, Reject)، اما فعلاً آن را عمومی نگه می‌داریم
    // و منطق اصلی در Handler بر اساس منبع و کاربر خواهد بود.
    public ManageBookingRequestForHotelRequirement()
    {
    }
}