// src/HotelReservation.Application/Features/Hotels/Commands/UpdateHotel/UpdateHotelCommandValidator.cs
using FluentValidation;
using System;

namespace HotelReservation.Application.Features.Hotels.Commands.UpdateHotel;

public class UpdateHotelCommandValidator : AbstractValidator<UpdateHotelCommand>
{
    public UpdateHotelCommandValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty().WithMessage("شناسه هتل الزامی است.")
            .NotEqual(Guid.Empty).WithMessage("شناسه هتل معتبر نیست.");

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("نام هتل الزامی است.")
            .MaximumLength(150).WithMessage("نام هتل نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.");

        RuleFor(p => p.Address)
            .NotEmpty().WithMessage("آدرس هتل الزامی است.")
            .MaximumLength(500).WithMessage("آدرس هتل نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.");

        RuleFor(p => p.ContactPerson)
            .MaximumLength(100).WithMessage("نام فرد رابط نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.")
            .When(p => !string.IsNullOrWhiteSpace(p.ContactPerson));

        RuleFor(p => p.PhoneNumber)
            .MaximumLength(20).WithMessage("شماره تلفن نمی‌تواند بیشتر از {MaxLength} کاراکتر باشد.")
            .When(p => !string.IsNullOrWhiteSpace(p.PhoneNumber));
    }
}