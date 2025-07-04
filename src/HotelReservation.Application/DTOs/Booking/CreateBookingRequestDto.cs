// src/HotelReservation.Application/DTOs/Booking/CreateBookingRequestDto.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelReservation.Application.DTOs.Booking;


public class CreateBookingRequestDto
{
    [Required(ErrorMessage = "کد ملی کارمند درخواست‌دهنده اصلی الزامی است.")]
    [MaxLength(10)]
    [RegularExpression("^[0-9]{10}$", ErrorMessage = "کد ملی کارمند باید ۱۰ رقمی و فقط شامل اعداد باشد.")]
    public string RequestingEmployeeNationalCode { get; set; }

    [Required(ErrorMessage = "دوره زمانی رزرو الزامی است (مثلاً 'بهار ۱۴۰۴').")]
    [MaxLength(100)]
    public Guid BookingPeriodId { get; set; }

    [Required(ErrorMessage = "تاریخ ورود الزامی است.")]
    public DateTime? CheckInDate { get; set; } // <<-- اصلاح شد

    [Required(ErrorMessage = "تاریخ خروج الزامی است.")]
    public DateTime? CheckOutDate { get; set; } // <<-- اصلاح شد

    [Required(ErrorMessage = "شناسه هتل الزامی است.")]
    public Guid HotelId { get; set; }

    [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از ۱۰۰۰ کاراکتر باشد.")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "لیست مهمانان الزامی است.")]
    [MinLength(1, ErrorMessage = "حداقل یک مهمان باید در درخواست وجود داشته باشد.")]
    public List<BookingGuestInputDto> Guests { get; set; } = new List<BookingGuestInputDto>();
}

