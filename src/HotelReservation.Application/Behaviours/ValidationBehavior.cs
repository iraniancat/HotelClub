// src/HotelReservation.Application/Behaviours/ValidationBehavior.cs
using FluentValidation; // برای IValidator و ValidationException
using MediatR; // برای IPipelineBehavior و IRequest
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HotelReservation.Application.Behaviours;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse> // محدود می‌کنیم که فقط برای IRequestها اعمال شود نه INotificationها
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any()) // اگر هیچ Validatorای برای این TRequest ثبت نشده باشد، از این مرحله رد می‌شویم
        {
            var context = new ValidationContext<TRequest>(request);

            // اجرای تمام Validatorهای پیدا شده برای TRequest به صورت موازی
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            // جمع‌آوری تمام خطاهای اعتبارسنجی از نتایج
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any()) // اگر خطایی وجود داشت
            {
                // یک ValidationException از خود FluentValidation پرتاب می‌کنیم
                // این Exception شامل لیستی از خطاها (failures) است.
                throw new ValidationException(failures);
            }
        }

        // اگر خطایی نبود یا هیچ Validatorای وجود نداشت، به Handler بعدی در pipeline یا Handler اصلی می‌رویم.
        return await next();
    }
}