// src/HotelReservation.Application/Features/Provinces/Queries/GetAllProvinces/GetAllProvincesQuery.cs
using HotelReservation.Application.DTOs.Location; // برای ProvinceDto
using MediatR;
using System.Collections.Generic;

namespace HotelReservation.Application.Features.Provinces.Queries.GetAllProvinces;

public class GetAllProvincesQuery : IRequest<IEnumerable<ProvinceDto>>
{
    // این Query پارامتری ندارد
}