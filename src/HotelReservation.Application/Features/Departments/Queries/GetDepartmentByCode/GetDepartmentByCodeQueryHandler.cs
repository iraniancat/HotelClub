// src/HotelReservation.Application/Features/Departments/Queries/GetDepartmentByCode/GetDepartmentByCodeQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.DTOs.Location;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Departments.Queries.GetDepartmentByCode;

public class GetDepartmentByCodeQueryHandler : IRequestHandler<GetDepartmentByCodeQuery, DepartmentDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDepartmentByCodeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<DepartmentDto?> Handle(GetDepartmentByCodeQuery request, CancellationToken cancellationToken)
    {
        // از متد GetByStringIdAsync که در IGenericRepository اضافه کردیم استفاده می‌کنیم
        var department = await _unitOfWork.DepartmentRepository.GetByStringIdAsync(request.Code); 
                                                                     // اگر کدها case-sensitive نیستند، .ToUpper() یا .ToLower() را در نظر بگیرید

        if (department == null)
        {
            return null;
        }

        return new DepartmentDto
        {
            Code = department.Code,
            Name = department.Name
        };
    }
}