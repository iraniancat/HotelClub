// src/HotelReservation.Application/Features/Hotels/Commands/DeleteHotel/DeleteHotelCommandValidator.cs
using FluentValidation;
using System;

namespace HotelReservation.Application.Features.Hotels.Commands.DeleteHotel;

public class DeleteHotelCommandValidator : AbstractValidator<DeleteHotelCommand>
{
    public DeleteHotelCommandValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty().WithMessage("شناسه هتل برای حذف الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه هتل معتبر نیست.");
    }
}