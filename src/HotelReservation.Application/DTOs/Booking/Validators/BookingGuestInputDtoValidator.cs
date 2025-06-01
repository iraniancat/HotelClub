// src/HotelReservation.Application/DTOs/Booking/Validators/BookingGuestInputDtoValidator.cs
using FluentValidation;
using HotelReservation.Application.DTOs.Booking; // برای BookingGuestInputDto

namespace HotelReservation.Application.DTOs.Booking.Validators;

public class BookingGuestInputDtoValidator : AbstractValidator<BookingGuestInputDto>
{
    public BookingGuestInputDtoValidator()
    {
        RuleFor(g => g.FullName)
            .NotEmpty().WithMessage("نام کامل مهمان الزامی است.")
            .MaximumLength(200).WithMessage("نام کامل مهمان نمی‌تواند بیشتر از ۲۰۰ کاراکتر باشد.");

        RuleFor(g => g.NationalCode)
            .NotEmpty().WithMessage("کد ملی مهمان الزامی است.")
            .Length(10).WithMessage("کد ملی مهمان باید ۱۰ رقم باشد.")
            .Matches("^[0-9]*$").WithMessage("کد ملی مهمان فقط می‌تواند شامل ارقام باشد.");

        RuleFor(g => g.RelationshipToEmployee)
            .NotEmpty().WithMessage("نسبت مهمان با کارمند اصلی الزامی است.")
            .MaximumLength(50).WithMessage("نسبت مهمان نمی‌تواند بیشتر از ۵۰ کاراکتر باشد.");
    }
}