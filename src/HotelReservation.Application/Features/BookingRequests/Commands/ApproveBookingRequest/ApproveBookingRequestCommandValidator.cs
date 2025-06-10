// src/HotelReservation.Application/Features/BookingRequests/Commands/ApproveBookingRequest/ApproveBookingRequestCommandValidator.cs
using FluentValidation;
using System;

namespace HotelReservation.Application.Features.BookingRequests.Commands.ApproveBookingRequest;

public class ApproveBookingRequestCommandValidator : AbstractValidator<ApproveBookingRequestCommand>
{
    public ApproveBookingRequestCommandValidator()
    {
        RuleFor(p => p.BookingRequestId)
            .NotEmpty().WithMessage("شناسه درخواست رزرو الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه درخواست رزرو معتبر نیست.");

       
        RuleFor(p => p.Comments)
            .MaximumLength(500).WithMessage("نظرات تأیید نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.")
            .When(p => !string.IsNullOrEmpty(p.Comments));
    }
}