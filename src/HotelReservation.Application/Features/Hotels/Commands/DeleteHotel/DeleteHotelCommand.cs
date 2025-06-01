// src/HotelReservation.Application/Features/Hotels/Commands/DeleteHotel/DeleteHotelCommand.cs
using MediatR; // برای IRequest
using System;   // برای Guid

namespace HotelReservation.Application.Features.Hotels.Commands.DeleteHotel;

public class DeleteHotelCommand : IRequest // یا IRequest<Unit>
{
    public Guid Id { get; } // شناسه هتلی که باید حذف شود

    public DeleteHotelCommand(Guid id)
    {
        Id = id;
    }
}