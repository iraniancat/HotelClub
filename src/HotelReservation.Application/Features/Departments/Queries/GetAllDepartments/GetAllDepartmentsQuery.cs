// src/HotelReservation.Application/Features/Departments/Queries/GetAllDepartments/GetAllDepartmentsQuery.cs
using HotelReservation.Application.DTOs.Location; // برای DepartmentDto
using MediatR;
using System.Collections.Generic;

namespace HotelReservation.Application.Features.Departments.Queries.GetAllDepartments;

public class GetAllDepartmentsQuery : IRequest<IEnumerable<DepartmentDto>>
{
    // این Query پارامتری ندارد
}