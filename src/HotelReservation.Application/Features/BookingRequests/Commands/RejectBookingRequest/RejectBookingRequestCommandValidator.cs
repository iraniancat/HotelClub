// src/HotelReservation.Application/Features/BookingRequests/Commands/RejectBookingRequest/RejectBookingRequestCommandValidator.cs
using FluentValidation;
using System;

namespace HotelReservation.Application.Features.BookingRequests.Commands.RejectBookingRequest;

public class RejectBookingRequestCommandValidator : AbstractValidator<RejectBookingRequestCommand>
{
    public RejectBookingRequestCommandValidator()
    {
        RuleFor(p => p.BookingRequestId)
            .NotEmpty().WithMessage("شناسه درخواست رزرو الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه درخواست رزرو معتبر نیست.");

        RuleFor(p => p.HotelUserId)
            .NotEmpty().WithMessage("شناسه کاربر هتل الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه کاربر هتل معتبر نیست.");

        RuleFor(p => p.RejectionReason)
            .NotEmpty().WithMessage("دلیل رد درخواست الزامی است.")
            .MaximumLength(500).WithMessage("دلیل رد درخواست نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.");
    }
}