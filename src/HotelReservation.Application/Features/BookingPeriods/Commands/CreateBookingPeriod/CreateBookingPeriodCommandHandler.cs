using HotelReservation.Domain.Entities;
using MediatR;

namespace HotelReservation.Application.Features.BookingPeriods.Commands.CreateBookingPeriod;

// CreateBookingPeriodCommandHandler.cs
public class CreateBookingPeriodCommandHandler : IRequestHandler<CreateBookingPeriodCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateBookingPeriodCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Guid> Handle(CreateBookingPeriodCommand request, CancellationToken cancellationToken)
    {
        var bookingPeriod = new BookingPeriod(request.Name, request.StartDate, request.EndDate, request.IsActive);
        await _unitOfWork.BookingPeriodRepository.AddAsync(bookingPeriod);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return bookingPeriod.Id;
    }
}