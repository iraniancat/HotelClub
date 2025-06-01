// src/HotelReservation.Application/Features/Provinces/Queries/GetProvinceByCode/GetProvinceByCodeQuery.cs
using HotelReservation.Application.DTOs.Location; // برای ProvinceDto
using MediatR;

namespace HotelReservation.Application.Features.Provinces.Queries.GetProvinceByCode;

public class GetProvinceByCodeQuery : IRequest<ProvinceDto?>
{
    public string Code { get; }

    public GetProvinceByCodeQuery(string code)
    {
        Code = code;
    }
}