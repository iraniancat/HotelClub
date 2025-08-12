namespace HotelReservation.Application.Features.UserManagement.Queries.GetUserWithDependents;

using HotelReservation.Application.DTOs.UserManagement;
using MediatR;
using System;

public class GetUserWithDependentsQuery : IRequest<UserWithDependentsDto?>
{
    public Guid UserId { get; }

    public GetUserWithDependentsQuery(Guid userId)
    {
        UserId = userId;
    }
}