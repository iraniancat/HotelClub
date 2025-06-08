// src/HotelReservation.Application/Features/BookingRequests/Commands/DeleteBookingFile/DeleteBookingFileCommandValidator.cs
using FluentValidation;
using System;

namespace HotelReservation.Application.Features.BookingRequests.Commands.DeleteBookingFile;

public class DeleteBookingFileCommandValidator : AbstractValidator<DeleteBookingFileCommand>
{
    public DeleteBookingFileCommandValidator()
    {
        RuleFor(p => p.BookingRequestId)
            .NotEmpty().WithMessage("شناسه درخواست رزرو الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه درخواست رزرو معتبر نیست.");

        RuleFor(p => p.FileId)
            .NotEmpty().WithMessage("شناسه فایل الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه فایل معتبر نیست.");
    }
}