// 1. فایل جدید: src/HotelReservation.Application/Features/Hotels/Queries/GetAllHotelsSimple/GetAllHotelsSimpleQuery.cs
namespace HotelReservation.Application.Features.Hotels.Queries.GetAllHotelsSimple;

using HotelReservation.Application.DTOs.Hotel;
using MediatR;
using System.Collections.Generic;

public class GetAllHotelsSimpleQuery : IRequest<IEnumerable<HotelDto>>
{
}