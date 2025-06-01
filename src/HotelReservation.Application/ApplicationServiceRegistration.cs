// src/HotelReservation.Application/ApplicationServiceRegistration.cs
  using FluentValidation; // برای AddValidatorsFromAssembly
  using HotelReservation.Application.Behaviours; // برای ValidationBehavior
  using MediatR; // برای IPipelineBehavior
  using Microsoft.Extensions.DependencyInjection;
  using System.Reflection;

  namespace HotelReservation.Application;

  public static class ApplicationServiceRegistration
  {
      public static IServiceCollection AddApplicationServices(this IServiceCollection services)
      {
          // ثبت AutoMapper (اگر در آینده اضافه شود)
          // services.AddAutoMapper(Assembly.GetExecutingAssembly());

          // ثبت Validatorهای FluentValidation از اسمبلی جاری (Application)
          services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

          // ثبت MediatR و Handlerهای آن از اسمبلی جاری (Application)
          services.AddMediatR(cfg => {
              cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
              
              // ثبت Pipeline Behavior برای اعتبارسنجی
              // این باید بعد از RegisterServicesFromAssembly باشد تا به درستی کار کند
              // یا به عنوان یک سرویس جداگانه ثبت شود. MediatR خودش پیدایشان می‌کند.
          });
          
          // ثبت ValidationBehavior به عنوان یک Pipeline Behavior
          // MediatR به طور خودکار Pipeline Behaviorهای ثبت شده را پیدا و اعمال می‌کند.
          // ترتیب ثبت Pipeline Behaviorها مهم است اگر به یکدیگر وابسته باشند.
          services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));


          return services;
      }
  }