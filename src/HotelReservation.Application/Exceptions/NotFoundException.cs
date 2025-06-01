// src/HotelReservation.Application/Exceptions/NotFoundException.cs
using System;

namespace HotelReservation.Application.Exceptions;

public class NotFoundException : ApplicationException
{
    public NotFoundException(string name, object key)
        : base($"موجودیت \"{name}\" با شناسه ({key}) یافت نشد.")
    {
    }
}