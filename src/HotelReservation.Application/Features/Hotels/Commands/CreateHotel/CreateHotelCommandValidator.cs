// src/HotelReservation.Application/Features/Hotels/Commands/CreateHotel/CreateHotelCommandValidator.cs
using FluentValidation;

namespace HotelReservation.Application.Features.Hotels.Commands.CreateHotel;

public class CreateHotelCommandValidator : AbstractValidator<CreateHotelCommand>
{
    public CreateHotelCommandValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("{PropertyName} هتل الزامی است.")
            .NotNull()
            .MaximumLength(150).WithMessage("{PropertyName} هتل نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.");

        RuleFor(p => p.Address)
            .NotEmpty().WithMessage("{PropertyName} هتل الزامی است.")
            .NotNull()
            .MaximumLength(500).WithMessage("{PropertyName} هتل نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.");

        RuleFor(p => p.ContactPerson)
            .MaximumLength(100).WithMessage("نام {PropertyName} نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.")
            .When(p => !string.IsNullOrWhiteSpace(p.ContactPerson)); // قانون فقط زمانی اعمال شود که مقدار خالی نباشد

        RuleFor(p => p.PhoneNumber)
            .MaximumLength(20).WithMessage("{PropertyName} نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.")
            // مثال برای یک قانون پیچیده‌تر (فعلاً کامنت شده):
            // .Matches(@"^\+?[0-9\s-()]*$").WithMessage("{PropertyName} معتبر نیست.")
            .When(p => !string.IsNullOrWhiteSpace(p.PhoneNumber));
    }
}