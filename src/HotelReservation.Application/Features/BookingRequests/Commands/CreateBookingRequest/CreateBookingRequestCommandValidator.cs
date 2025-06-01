// src/HotelReservation.Application/Features/BookingRequests/Commands/CreateBookingRequest/CreateBookingRequestCommandValidator.cs
using FluentValidation;
using HotelReservation.Application.DTOs.Booking.Validators; // برای BookingGuestInputDtoValidator
using System;

namespace HotelReservation.Application.Features.BookingRequests.Commands.CreateBookingRequest;

public class CreateBookingRequestCommandValidator : AbstractValidator<CreateBookingRequestCommand>
{
    public CreateBookingRequestCommandValidator()
    {
        RuleFor(p => p.RequestSubmitterUserId)
            .NotEmpty().WithMessage("شناسه کاربر ثبت کننده درخواست الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه کاربر ثبت کننده درخواست معتبر نیست.");

        RuleFor(p => p.RequestingEmployeeNationalCode)
            .NotEmpty().WithMessage("کد ملی کارمند درخواست‌دهنده اصلی الزامی است.")
            .Length(10).WithMessage("کد ملی کارمند درخواست‌دهنده باید ۱۰ رقم باشد.")
            .Matches("^[0-9]*$").WithMessage("کد ملی کارمند درخواست‌دهنده فقط می‌تواند شامل ارقام باشد.");

        RuleFor(p => p.BookingPeriod)
            .NotEmpty().WithMessage("دوره زمانی رزرو الزامی است.")
            .MaximumLength(100).WithMessage("دوره زمانی رزرو نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");

        RuleFor(p => p.CheckInDate)
            .NotEmpty().WithMessage("تاریخ ورود الزامی است.")
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("تاریخ ورود نمی‌تواند مربوط به گذشته باشد."); // تاریخ ورود باید امروز یا در آینده باشد

        RuleFor(p => p.CheckOutDate)
            .NotEmpty().WithMessage("تاریخ خروج الزامی است.")
            .GreaterThan(p => p.CheckInDate).WithMessage("تاریخ خروج باید بعد از تاریخ ورود باشد.");

        RuleFor(p => p.HotelId)
            .NotEmpty().WithMessage("شناسه هتل الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه هتل معتبر نیست.");

        RuleFor(p => p.Notes)
            .MaximumLength(1000).WithMessage("توضیحات نمی‌تواند بیشتر از ۱۰۰۰ کاراکتر باشد.")
            .When(p => !string.IsNullOrEmpty(p.Notes));

        RuleFor(p => p.Guests)
            .NotEmpty().WithMessage("لیست مهمانان نمی‌تواند خالی باشد.")
            .Must(guests => guests.Count >= 1).WithMessage("حداقل یک مهمان باید در درخواست وجود داشته باشد.");

        // اعتبارسنجی هر آیتم در لیست مهمانان
        RuleForEach(p => p.Guests)
            .SetValidator(new BookingGuestInputDtoValidator());
    }
}