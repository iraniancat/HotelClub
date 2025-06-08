// src/HotelReservation.Application/Features/Hotels/Commands/CreateHotel/CreateHotelCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence; // برای IUnitOfWork
using HotelReservation.Domain.Entities; // برای موجودیت Hotel
using MediatR; // برای IRequestHandler
using Microsoft.Extensions.Logging;
using System; // برای Guid
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Hotels.Commands.CreateHotel;

public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateHotelCommandHandler> _logger; // اگر از AutoMapper استفاده می‌کردیم

    public CreateHotelCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateHotelCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        // ... (بخش اعتبارسنجی و ایجاد hotelEntity همانند قبل) ...
        var hotelEntity = new Hotel(
            request.Name,
            request.Address,
            request.ContactPerson,
            request.PhoneNumber
        );

        // برای دسترسی به Database از طریق UnitOfWork، باید آن را expose کنید یا این منطق را به UnitOfWork منتقل کنید.
        // فرض می‌کنیم UnitOfWork یک متد برای شروع تراکنش دارد یا Context را expose می‌کند.
        // این یک مثال ساده شده است:
        // AppDbContext dbContextForTransaction = _unitOfWork.GetDbContext(); // متد فرضی
        // await using (var transaction = await dbContextForTransaction.Database.BeginTransactionAsync(cancellationToken))
        // {
        //     try
        //     {
        //         await _unitOfWork.HotelRepository.AddAsync(hotelEntity);
        //         _logger.LogInformation("Attempting to save changes for new hotel: {HotelName} within transaction.", hotelEntity.Name);
        //         int result = await _unitOfWork.SaveChangesAsync(cancellationToken);
        //         _logger.LogInformation("SaveChangesAsync result within transaction: {ResultCount}", result);

        //         if (result > 0)
        //         {
        //             await transaction.CommitAsync(cancellationToken);
        //             _logger.LogInformation("Transaction committed for hotel: {HotelName}", hotelEntity.Name);
        //         }
        //         else
        //         {
        //             _logger.LogWarning("No changes saved, rolling back transaction for hotel: {HotelName}", hotelEntity.Name);
        //             await transaction.RollbackAsync(cancellationToken);
        //             // شاید بخواهید اینجا یک Exception پرتاب کنید یا یک مقدار خاص برگردانید
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error during transaction for hotel: {HotelName}. Rolling back.", hotelEntity.Name);
        //         await transaction.RollbackAsync(cancellationToken);
        //         throw; // دوباره پرتاب خطا
        //     }
        // }
        // return hotelEntity.Id;

        // کد ساده‌تر فعلی شما (بدون تراکنش صریح در Handler):
        await _unitOfWork.HotelRepository.AddAsync(hotelEntity);
        _logger.LogInformation("Attempting to save changes for new hotel: {HotelName}", hotelEntity.Name);
        int result = await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("SaveChangesAsync result: {NumberOfStateEntriesWritten}", result);

        if (result == 0)
        {
            _logger.LogWarning("SaveChangesAsync returned 0. Hotel {HotelName} was NOT saved to the database.", hotelEntity.Name);
            // اینجا می‌توانید یک Exception پرتاب کنید تا UI متوجه مشکل شود
            throw new ApplicationException("خطا در ذخیره‌سازی هتل. هیچ تغییری در پایگاه داده اعمال نشد.");
        }
        _logger.LogInformation("Hotel {HotelName} with ID {HotelId} successfully processed by SaveChanges.", hotelEntity.Name, hotelEntity.Id);
        return hotelEntity.Id;
    }
}