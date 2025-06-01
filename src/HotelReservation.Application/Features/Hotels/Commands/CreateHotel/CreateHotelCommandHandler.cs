// src/HotelReservation.Application/Features/Hotels/Commands/CreateHotel/CreateHotelCommandHandler.cs
using HotelReservation.Application.Contracts.Persistence; // برای IUnitOfWork
using HotelReservation.Domain.Entities; // برای موجودیت Hotel
using MediatR; // برای IRequestHandler
using System; // برای Guid
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Features.Hotels.Commands.CreateHotel;

public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    // private readonly IMapper _mapper; // اگر از AutoMapper استفاده می‌کردیم

    public CreateHotelCommandHandler(IUnitOfWork unitOfWork /*, IMapper mapper*/)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        // _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<Guid> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        // 1. (اختیاری) اعتبارسنجی بیشتر یا بررسی قوانین کسب‌وکار پیچیده
        //    اعتبارسنجی اولیه توسط FluentValidation (از طریق Pipeline Behavior) انجام خواهد شد.
        //    مثلاً: بررسی اینکه آیا هتلی با همین نام و آدرس قبلاً وجود دارد یا خیر (از طریق _unitOfWork.HotelRepository)
        //    var existingHotel = await _unitOfWork.HotelRepository.FindByNameAsync(request.Name);
        //  if (existingHotel.Any(h => h.Address == request.Address))
          //  {
        // //        // پرتاب یک Exception سفارشی یا بازگرداندن یک نتیجه خطا
        // //        throw new Exceptions.BadRequestException($"هتلی با نام '{request.Name}' و آدرس '{request.Address}' قبلاً ثبت شده است.");
        // //    }


        // 2. ایجاد نمونه از موجودیت Hotel
        //    در اینجا از سازنده موجودیت Hotel که قبلاً تعریف کردیم، استفاده می‌کنیم.
        var hotelEntity = new Hotel(
            request.Name,
            request.Address,
            request.ContactPerson,
            request.PhoneNumber
        );
        // اگر از AutoMapper استفاده می‌کردیم:
        // var hotelEntity = _mapper.Map<Hotel>(request);


        // 3. افزودن موجودیت جدید به Repository
        await _unitOfWork.HotelRepository.AddAsync(hotelEntity);


        // 4. ذخیره تغییرات در پایگاه داده از طریق UnitOfWork
        //    این عمل تمام تغییرات انجام شده در DbContext فعلی (از طریق تمام Repositoryهای این UnitOfWork) را
        //    در یک تراکنش به پایگاه داده ارسال می‌کند.
        await _unitOfWork.SaveChangesAsync(cancellationToken);


        // 5. بازگرداندن شناسه هتل ایجاد شده
        return hotelEntity.Id;
    }
}