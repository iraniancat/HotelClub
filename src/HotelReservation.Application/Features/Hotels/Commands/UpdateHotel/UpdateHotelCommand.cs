// src/HotelReservation.Application/Features/Hotels/Commands/UpdateHotel/UpdateHotelCommand.cs
using MediatR; // برای IRequest
using System;   // برای Guid

namespace HotelReservation.Application.Features.Hotels.Commands.UpdateHotel;

public class UpdateHotelCommand : IRequest // یا IRequest<Unit> - به معنی اینکه هیچ مقدار خاصی بازگردانده نمی‌شود
{
    public Guid Id { get; set; } // شناسه هتلی که باید به‌روز شود
    public string Name { get; set; }
    public string Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? PhoneNumber { get; set; }

    public UpdateHotelCommand(Guid id, string name, string address, string? contactPerson, string? phoneNumber)
    {
        Id = id;
        Name = name;
        Address = address;
        ContactPerson = contactPerson;
        PhoneNumber = phoneNumber;
    }
}