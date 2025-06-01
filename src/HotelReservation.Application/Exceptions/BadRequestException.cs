// src/HotelReservation.Application/Exceptions/BadRequestException.cs
using System;

namespace HotelReservation.Application.Exceptions;

public class BadRequestException : ApplicationException
{
    public BadRequestException(string message) : base(message)
    {
    }

    public BadRequestException(string message, Exception innerException) : base(message, innerException)
    {
    }
}