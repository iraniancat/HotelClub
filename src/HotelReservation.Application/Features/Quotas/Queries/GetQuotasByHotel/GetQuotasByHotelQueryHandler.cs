using HotelReservation.Application.DTOs.Quota;
using HotelReservation.Domain.Entities;
using MediatR;

namespace HotelReservation.Application.Features.Quotas.Queries.GetQuotasByHotel;

public class GetQuotasByHotelQueryHandler : IRequestHandler<GetQuotasByHotelQuery, IEnumerable<ProvinceHotelQuotaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetQuotasByHotelQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IEnumerable<ProvinceHotelQuotaDto>> Handle(GetQuotasByHotelQuery request, CancellationToken cancellationToken)
    {
        // ابتدا تمام استان‌ها را می‌خوانیم
        var allProvinces = await _unitOfWork.ProvinceRepository.GetAllAsync();

        // سپس سهمیه‌های تعریف شده برای هتل مورد نظر را می‌خوانیم
        var definedQuotas = await _unitOfWork.ProvinceHotelQuotaRepository
            .GetAsyncWithIncludes(q => q.HotelId == request.HotelId, includes: new List<System.Linq.Expressions.Expression<Func<ProvinceHotelQuota, object>>> { q => q.Hotel });

        var definedQuotasDict = definedQuotas.ToDictionary(q => q.ProvinceCode);

        // یک لیست کامل از تمام استان‌ها می‌سازیم و سهمیه آن‌ها را (اگر وجود داشت) مشخص می‌کنیم
        var result = allProvinces.Select(province =>
        {
            var hasQuota = definedQuotasDict.TryGetValue(province.Code, out var quota);
            return new ProvinceHotelQuotaDto
            {
                Id = hasQuota ? quota!.Id : Guid.Empty,
                ProvinceCode = province.Code,
                ProvinceName = province.Name,
                HotelId = request.HotelId,
                HotelName = hasQuota ? quota!.Hotel.Name : string.Empty,
                RoomLimit = hasQuota ? quota!.RoomLimit : 0 // اگر سهمیه تعریف نشده، ۰ در نظر می‌گیریم
            };
        }).OrderBy(p => p.ProvinceName).ToList();

        return result;
    }
}