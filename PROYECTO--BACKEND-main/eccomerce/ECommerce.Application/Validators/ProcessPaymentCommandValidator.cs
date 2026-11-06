using ECommerce.Application.Commands.ProcessPayment;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0).WithMessage("El ID de la orden es obligatorio.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("El ID de usuario es obligatorio.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("El monto debe ser mayor a cero.");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("El método de pago es obligatorio.");

        RuleFor(x => x.CardNumber)
            .NotEmpty().WithMessage("El número de tarjeta es obligatorio.");

        RuleFor(x => x.ExpiryDate)
            .NotEmpty().WithMessage("La fecha de vencimiento es obligatoria.");

        RuleFor(x => x.Cvv)
            .NotEmpty().WithMessage("El código CVV es obligatorio.");
    }
}

