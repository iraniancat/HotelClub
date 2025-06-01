// src/HotelReservation.Application/Features/Departments/Queries/GetDepartmentByCode/GetDepartmentByCodeQuery.cs
using HotelReservation.Application.DTOs.Location; // برای DepartmentDto
using MediatR;

namespace HotelReservation.Application.Features.Departments.Queries.GetDepartmentByCode;

public class GetDepartmentByCodeQuery : IRequest<DepartmentDto?>
{
    public string Code { get; }

    public GetDepartmentByCodeQuery(string code)
    {
        Code = code;
    }
}