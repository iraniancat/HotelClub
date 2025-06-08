// UpdateBookingPeriodCommandHandler.cs
using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;

namespace HotelReservation.Application.Features.BookingPeriods.Commands.UpdateBookingPeriod;

// UpdateBookingPeriodCommandHandler.cs
public class UpdateBookingPeriodCommandHandler : IRequestHandler<UpdateBookingPeriodCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public UpdateBookingPeriodCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task Handle(UpdateBookingPeriodCommand request, CancellationToken cancellationToken)
    {
        var periodToUpdate = await _unitOfWork.BookingPeriodRepository.GetByIdAsync(request.Id);
        if (periodToUpdate == null) throw new NotFoundException(nameof(BookingPeriod), request.Id);

        // Validator یکتایی نام را بررسی می‌کند.

        periodToUpdate.UpdateDetails(request.Name, request.StartDate, request.EndDate, request.IsActive);
        await _unitOfWork.BookingPeriodRepository.UpdateAsync(periodToUpdate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
