using FluentValidation;

namespace HotelReservation.Application.Features.Rooms.Commands.CreateRoom;

public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreateRoomCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        RuleFor(p => p.HotelId).NotEmpty().NotEqual(Guid.Empty);
        RuleFor(p => p.RoomNumber)
            .NotEmpty()
            .MaximumLength(20)
            .MustAsync(async (command, roomNumber, token) =>
                await _unitOfWork.RoomRepository.IsRoomNumberUniqueAsync(command.HotelId, roomNumber))
            .WithMessage("اتاقی با این شماره در این هتل قبلاً ثبت شده است.");
        // سایر قوانین برای Capacity و PricePerNight
    }
}
