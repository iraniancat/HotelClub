using HotelReservation.Application.Exceptions;
using HotelReservation.Domain.Entities;
using MediatR;

namespace HotelReservation.Application.Features.Quotas.Commands.SetQuota;

public class SetQuotaCommandHandler : IRequestHandler<SetQuotaCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    public SetQuotaCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Guid> Handle(SetQuotaCommand request, CancellationToken cancellationToken)
    {
        // بررسی وجود هتل و استان
        if (!await _unitOfWork.HotelRepository.ExistsAsync(request.HotelId))
            throw new NotFoundException(nameof(Hotel), request.HotelId);
        if (!await _unitOfWork.ProvinceRepository.ExistsByCodeAsync(request.ProvinceCode))
            throw new NotFoundException(nameof(Province), request.ProvinceCode);

        // بررسی اینکه آیا سهمیه از قبل برای این استان-هتل وجود دارد یا خیر
        var existingQuota = (await _unitOfWork.ProvinceHotelQuotaRepository
            .GetAsync(q => q.HotelId == request.HotelId && q.ProvinceCode == request.ProvinceCode))
            .FirstOrDefault();

        if (existingQuota != null)
        {
            // اگر وجود داشت، ویرایش می‌کنیم
            existingQuota.SetRoomLimit(request.RoomLimit);
            await _unitOfWork.ProvinceHotelQuotaRepository.UpdateAsync(existingQuota);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return existingQuota.Id;
        }
        else
        {
            // اگر وجود نداشت، یک سهمیه جدید ایجاد می‌کنیم
            var newQuota = new ProvinceHotelQuota(request.ProvinceCode, request.HotelId, request.RoomLimit);
            await _unitOfWork.ProvinceHotelQuotaRepository.AddAsync(newQuota);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return newQuota.Id;
        }
    }
}