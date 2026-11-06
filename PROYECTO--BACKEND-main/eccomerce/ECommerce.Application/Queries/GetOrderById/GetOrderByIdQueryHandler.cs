using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository _orders;

    public GetOrderByIdQueryHandler(IOrderRepository orders) => _orders = orders;

    public async Task<OrderDto?> Handle(GetOrderByIdQuery query, CancellationToken ct)
    {
        var order = await _orders.GetByIdWithItemsAsync(query.Id, ct);
        return order is null ? null : Map(order);
    }

    internal static OrderDto Map(Order order) =>
        new(
            order.Id,
            order.UserId,
            order.Status.ToString(),
            order.Total,
            order.CreatedAt,
            order.Items
                .Select(i => new OrderItemDto(i.ProductId, i.UnitPrice, i.Quantity, i.Subtotal))
                .ToList());
}
