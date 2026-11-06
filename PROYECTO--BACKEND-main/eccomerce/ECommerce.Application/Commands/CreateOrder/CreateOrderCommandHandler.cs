using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Queries.GetOrderById;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orders;
    private readonly IProductRepository _products;
    private readonly IPaymentClient _paymentClient;

    public CreateOrderCommandHandler(
        IOrderRepository orders,
        IProductRepository products,
        IPaymentClient paymentClient)
    {
        _orders = orders;
        _products = products;
        _paymentClient = paymentClient;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand command, CancellationToken ct)
    {
        // Una orden no puede crearse vacía; debe contener al menos un producto
        if (command.Items.Count == 0)
            throw new ArgumentException("La orden debe tener al menos un ítem.");

        // Instancia una nueva Orden asociándola al UserId. Por defecto nace con estado 'Pending' (Pendiente) y Total = 0
        var order = new Order(command.UserId);

        foreach (var line in command.Items)
        {
            var product = await _products.GetByIdAsync(line.ProductId, ct)
                ?? throw new InvalidOperationException($"Producto no encontrado: {line.ProductId}");

            order.AddItem(product, line.Quantity);
            // Actualiza el stock en la base de datos
            await _products.UpdateAsync(product, ct);
        }

        // Guarda la orden inicialmente en estado 'Pending'
        await _orders.AddAsync(order, ct);

        // Enviar solicitud de pago al microservicio externo PaymentService
        var paymentRequest = new ProcessPaymentRequestDto(
            order.Id,
            order.UserId,
            order.Total,
            "CreditCard",
            "4532111122223333",
            "12/28",
            "123");

        var paymentResult = await _paymentClient.ProcessPaymentAsync(paymentRequest, ct);

        if (paymentResult is not null)
        {
            if (string.Equals(paymentResult.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                order.Confirm();
                await _orders.UpdateAsync(order, ct);
            }
            else if (string.Equals(paymentResult.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                order.Cancel();
                await _orders.UpdateAsync(order, ct);
            }
        }

        return GetOrderByIdQueryHandler.Map(order);
    }
}

