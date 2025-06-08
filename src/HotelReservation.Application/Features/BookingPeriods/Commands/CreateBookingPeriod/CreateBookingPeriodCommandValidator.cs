using FluentValidation;

namespace HotelReservation.Application.Features.BookingPeriods.Commands.CreateBookingPeriod;

// CreateBookingPeriodCommandValidator.cs

public class CreateBookingPeriodCommandValidator : AbstractValidator<CreateBookingPeriodCommand>
{
    public CreateBookingPeriodCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("نام دوره زمانی الزامی است.")
            .MaximumLength(150).WithMessage("نام دوره زمانی نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.")
            .MustAsync(async (name, token) => !await unitOfWork.BookingPeriodRepository.ExistsByNameAsync(name))
            .WithMessage("دوره‌ای با این نام قبلاً ثبت شده است.");

        RuleFor(p => p.EndDate)
            .GreaterThan(p => p.StartDate).WithMessage("تاریخ پایان باید بعد از تاریخ شروع باشد.");
    }
}
