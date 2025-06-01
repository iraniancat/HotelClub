// src/HotelReservation.Application/Features/Hotels/Queries/GetHotelById/GetHotelByIdQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence; // برای IUnitOfWork
using HotelReservation.Application.DTOs.Hotel; // برای HotelDto
using HotelReservation.Domain.Entities; // برای Hotel (موجودیت دامنه)
using MediatR; // برای IRequestHandler
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Hotels.Queries.GetHotelById;

public class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, HotelDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    // private readonly IMapper _mapper; // اگر از AutoMapper استفاده می‌کردیم

    public GetHotelByIdQueryHandler(IUnitOfWork unitOfWork /*, IMapper mapper*/)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        // _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<HotelDto?> Handle(GetHotelByIdQuery request, CancellationToken cancellationToken)
    {
        // از متد GetHotelWithDetailsAsync که در IHotelRepository تعریف کردیم استفاده می‌کنیم
        // تا در صورت نیاز، اطلاعات مرتبط با هتل (مانند اتاق‌ها) را هم بارگذاری کنیم،
        // حتی اگر HotelDto فعلی آن‌ها را نمایش ندهد، برای آینده‌نگری خوب است.
        // یا می‌توانیم از GetByIdAsync ساده‌تر استفاده کنیم اگر جزئیات لازم نیست.
        var hotel = await _unitOfWork.HotelRepository.GetHotelWithDetailsAsync(request.Id);
        // var hotel = await _unitOfWork.HotelRepository.GetByIdAsync(request.Id); // گزینه ساده‌تر

        if (hotel == null)
        {
            return null; // یا می‌توان یک NotFoundException سفارشی پرتاب کرد
        }

        // نگاشت دستی از موجودیت Hotel به HotelDto
        // اگر از AutoMapper استفاده می‌کردیم: return _mapper.Map<HotelDto>(hotel);
        return new HotelDto
        {
            Id = hotel.Id,
            Name = hotel.Name,
            Address = hotel.Address,
            ContactPerson = hotel.ContactPerson,
            PhoneNumber = hotel.PhoneNumber
            // در آینده اگر Rooms به HotelDto اضافه شد:
            // Rooms = hotel.Rooms.Select(r => new RoomDto { Id = r.Id, RoomNumber = r.RoomNumber, ... }).ToList()
        };
    }
}