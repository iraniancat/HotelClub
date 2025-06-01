// src/HotelReservation.Application/Exceptions/ForbiddenAccessException.cs
using System;

namespace HotelReservation.Application.Exceptions;

public class ForbiddenAccessException : ApplicationException
{
    public ForbiddenAccessException() : base("شما مجاز به انجام این عملیات نیستید.") { }
    public ForbiddenAccessException(string message) : base(message) { }
}