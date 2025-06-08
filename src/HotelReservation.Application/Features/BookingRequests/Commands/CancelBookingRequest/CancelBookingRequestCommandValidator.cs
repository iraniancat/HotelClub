// src/HotelReservation.Application/Features/BookingRequests/Commands/CancelBookingRequest/CancelBookingRequestCommandValidator.cs
using FluentValidation;
using System;

namespace HotelReservation.Application.Features.BookingRequests.Commands.CancelBookingRequest;

public class CancelBookingRequestCommandValidator : AbstractValidator<CancelBookingRequestCommand>
{
    public CancelBookingRequestCommandValidator()
    {
        RuleFor(p => p.BookingRequestId)
            .NotEmpty().WithMessage("شناسه درخواست رزرو الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه درخواست رزرو معتبر نیست.");

        RuleFor(p => p.CancellationReason)
            .MaximumLength(500).WithMessage("دلیل لغو نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.")
            .When(p => !string.IsNullOrEmpty(p.CancellationReason));
    }
}