// src/HotelReservation.Application/Features/Hotels/Commands/DeleteHotel/DeleteHotelCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence;
using HotelReservation.Application.Exceptions; // برای NotFoundException
using HotelReservation.Domain.Entities; // برای nameof(Hotel)
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Hotels.Commands.DeleteHotel;

public class DeleteHotelCommandHandler : IRequestHandler<DeleteHotelCommand> // یا IRequestHandler<DeleteHotelCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteHotelCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task Handle(DeleteHotelCommand request, CancellationToken cancellationToken)
    // public async Task<Unit> Handle(DeleteHotelCommand request, CancellationToken cancellationToken) // اگر از IRequest<Unit> استفاده می‌کنیم
    {
        var hotelToDelete = await _unitOfWork.HotelRepository.GetByIdAsync(request.Id);

        if (hotelToDelete == null)
        {
            throw new NotFoundException(nameof(Hotel), request.Id);
        }

        // توجه: رفتار OnDelete برای روابط وابسته (مانند اتاق‌ها، رزروها) در AppDbContext پیکربندی شده است.
        // اگر Restrict باشد و رکوردهای وابسته وجود داشته باشند، پایگاه داده خطا خواهد داد.
        // این مورد باید در اینجا مدیریت شود یا به لایه بالاتر (مثلاً Controller با try-catch خاص) منتقل شود.
        // برای سادگی فعلاً فرض می‌کنیم که یا رکوردهای وابسته وجود ندارند یا Cascade Delete تنظیم شده
        // یا خطای پایگاه داده به Global Exception Handler می‌رسد.

        await _unitOfWork.HotelRepository.DeleteAsync(hotelToDelete);
        // یا می‌توانستیم از: await _unitOfWork.HotelRepository.DeleteByIdAsync(request.Id); استفاده کنیم
        // که در آن صورت بررسی hotelToDelete == null باید داخل DeleteByIdAsync انجام می‌شد یا دیگر لازم نبود.
        // رویکرد فعلی (خواندن سپس حذف) اجازه می‌دهد تا قبل از حذف، بررسی‌های دیگری نیز انجام شود.

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // return Unit.Value; // اگر از IRequest<Unit> استفاده می‌کنیم
    }
}