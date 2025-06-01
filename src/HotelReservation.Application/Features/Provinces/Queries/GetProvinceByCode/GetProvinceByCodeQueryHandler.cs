// src/HotelReservation.Application/Features/Provinces/Queries/GetProvinceByCode/GetProvinceByCodeQueryHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.DTOs.Location;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Provinces.Queries.GetProvinceByCode;

public class GetProvinceByCodeQueryHandler : IRequestHandler<GetProvinceByCodeQuery, ProvinceDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProvinceByCodeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ProvinceDto?> Handle(GetProvinceByCodeQuery request, CancellationToken cancellationToken)
    {
        // از متد GetByStringIdAsync که در IGenericRepository اضافه کردیم استفاده می‌کنیم
        var province = await _unitOfWork.ProvinceRepository.GetByStringIdAsync(request.Code.ToUpper()); 

        if (province == null)
        {
            return null;
        }

        return new ProvinceDto
        {
            Code = province.Code,
            Name = province.Name
        };
    }
}