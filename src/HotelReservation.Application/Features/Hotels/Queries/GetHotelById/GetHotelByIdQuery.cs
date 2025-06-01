// src/HotelReservation.Application/Features/Hotels/Queries/GetHotelById/GetHotelByIdQuery.cs
using HotelReservation.Application.DTOs.Hotel; // برای HotelDto
using MediatR; // برای IRequest
using System; // برای Guid

namespace HotelReservation.Application.Features.Hotels.Queries.GetHotelById;

public class GetHotelByIdQuery : IRequest<HotelDto?> // نتیجه می‌تواند HotelDto یا null باشد (اگر هتل پیدا نشود)
{
    public Guid Id { get; set; }

    public GetHotelByIdQuery(Guid id)
    {
        Id = id;
    }
}