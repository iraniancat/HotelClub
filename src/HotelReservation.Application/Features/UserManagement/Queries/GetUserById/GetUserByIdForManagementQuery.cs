// src/HotelReservation.Application/Features/UserManagement/Queries/GetUserById/GetUserByIdForManagementQuery.cs
using HotelReservation.Application.DTOs.UserManagement; // برای UserManagementDetailsDto
using MediatR; // برای IRequest
using System; // برای Guid

namespace HotelReservation.Application.Features.UserManagement.Queries.GetUserById;

public class GetUserByIdForManagementQuery : IRequest<UserManagementDetailsDto?>
{
    public Guid UserId { get; } // شناسه کاربری که جزئیاتش درخواست شده

    public GetUserByIdForManagementQuery(Guid userId)
    {
        UserId = userId;
    }
}