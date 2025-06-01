// src/HotelReservation.Application/Features/Hotels/Commands/UpdateHotel/UpdateHotelCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Domain.Entities;
using HotelReservation.Application.Exceptions; // برای NotFoundException
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Hotels.Commands.UpdateHotel;

public class UpdateHotelCommandHandler : IRequestHandler<UpdateHotelCommand> // یا IRequestHandler<UpdateHotelCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    // private readonly IMapper _mapper;

    public UpdateHotelCommandHandler(IUnitOfWork unitOfWork /*, IMapper mapper*/)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        // _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task Handle(UpdateHotelCommand request, CancellationToken cancellationToken) // برای IRequest، نوع بازگشتی Task است
    // public async Task<Unit> Handle(UpdateHotelCommand request, CancellationToken cancellationToken) // برای IRequest<Unit>
    {
        var hotelToUpdate = await _unitOfWork.HotelRepository.GetByIdAsync(request.Id);

        if (hotelToUpdate == null)
        {
            throw new NotFoundException(nameof(Hotel), request.Id);
        }

        // اگر از AutoMapper استفاده می‌کردیم:
        // _mapper.Map(request, hotelToUpdate, typeof(UpdateHotelCommand), typeof(Hotel));
        // نگاشت دستی:
        hotelToUpdate.UpdateDetails(
            request.Name,
            request.Address,
            request.ContactPerson,
            request.PhoneNumber
        );
        // متد UpdateDetails را در موجودیت Hotel داریم که این کار را انجام می‌دهد.

        // UpdateAsync از GenericRepository وضعیت موجودیت را Modified می‌کند.
        // اگر موجودیت توسط همین DbContext خوانده شده باشد، EF Core تغییرات را تشخیص می‌دهد
        // و نیازی به فراخوانی صریح UpdateAsync نیست، اما فراخوانی آن ضرری ندارد.
        await _unitOfWork.HotelRepository.UpdateAsync(hotelToUpdate); // این خط می‌تواند اختیاری باشد اگر UpdateDetails تغییرات را اعمال کند و موجودیت Track شده باشد.

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // return Unit.Value; // اگر از IRequest<Unit> استفاده می‌کنیم
        // اگر از IRequest استفاده می‌کنیم، return خاصی لازم نیست چون Task است.
    }
}