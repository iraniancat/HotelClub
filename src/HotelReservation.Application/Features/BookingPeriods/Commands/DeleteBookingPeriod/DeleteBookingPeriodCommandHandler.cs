// DeleteBookingPeriodCommandHandler.cs
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;

namespace HotelReservation.Application.Features.BookingPeriods.Commands.DeleteBookingPeriod;

// DeleteBookingPeriodCommandHandler.cs
public class DeleteBookingPeriodCommandHandler : IRequestHandler<DeleteBookingPeriodCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public DeleteBookingPeriodCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(DeleteBookingPeriodCommand request, CancellationToken cancellationToken)
    {
        var periodToDelete = await _unitOfWork.BookingPeriodRepository.GetByIdAsync(request.Id);
        if (periodToDelete == null) throw new NotFoundException(nameof(BookingPeriod), request.Id);

        // بررسی اینکه آیا رزروی در این دوره ثبت شده است یا خیر (OnDelete.Restrict این کار را در دیتابیس انجام می‌دهد)
        // اما بررسی در اینجا، پیام خطای بهتری به کاربر می‌دهد.
        var hasBookings = await _unitOfWork.BookingRequestRepository.GetAsync(b => b.BookingPeriodId == request.Id);
        if (hasBookings.Any())
        {
            throw new BadRequestException("امکان حذف این دوره زمانی وجود ندارد زیرا شامل درخواست‌های رزرو ثبت شده است.");
        }

        await _unitOfWork.BookingPeriodRepository.DeleteAsync(periodToDelete);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}