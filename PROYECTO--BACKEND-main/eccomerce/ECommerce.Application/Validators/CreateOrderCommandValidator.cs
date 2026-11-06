using ECommerce.Application.Commands.CreateOrder;
using FluentValidation;

namespace ECommerce.Application.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("El ID de usuario es obligatorio.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("La orden debe contener al menos un ítem.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("El ID del producto es obligatorio.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");
        });
    }
}
