// src/HotelReservation.Application/Features/Hotels/Commands/CreateHotel/CreateHotelCommand.cs
using MediatR; // برای IRequest
using System; // برای Guid

namespace HotelReservation.Application.Features.Hotels.Commands.CreateHotel;

public class CreateHotelCommand : IRequest<Guid> // این Command پس از اجرا، شناسه (Guid) هتل ایجاد شده را باز می‌گرداند
{
    // خصوصیاتی که از CreateHotelDto می‌آیند
    public string Name { get; set; }
    public string Address { get; set; }
    public string? ContactPerson { get; set; }
    public string? PhoneNumber { get; set; }

    // سازنده برای مقداردهی اولیه (اختیاری اما مفید)
    public CreateHotelCommand(string name, string address, string? contactPerson, string? phoneNumber)
    {
        Name = name;
        Address = address;
        ContactPerson = contactPerson;
        PhoneNumber = phoneNumber;
    }
}