// src/HotelReservation.Application/Features/Departments/Queries/GetAllDepartments/GetAllDepartmentsQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence; // برای IUnitOfWork
using HotelReservation.Application.DTOs.Location; // برای DepartmentDto
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Departments.Queries.GetAllDepartments;

public class GetAllDepartmentsQueryHandler : IRequestHandler<GetAllDepartmentsQuery, IEnumerable<DepartmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllDepartmentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IEnumerable<DepartmentDto>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var departments = await _unitOfWork.DepartmentRepository.GetAllAsync();

        if (departments == null || !departments.Any())
        {
            return new List<DepartmentDto>();
        }

        return departments.Select(department => new DepartmentDto
        {
            Code = department.Code,
            Name = department.Name
        }).OrderBy(d => d.Name).ToList();
    }
}