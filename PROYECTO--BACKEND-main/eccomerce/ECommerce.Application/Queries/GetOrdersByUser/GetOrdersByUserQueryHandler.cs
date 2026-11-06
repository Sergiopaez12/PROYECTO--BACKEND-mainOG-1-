using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Queries.GetOrdersByUser;

public class GetOrdersByUserQueryHandler : IRequestHandler<GetOrdersByUserQuery, IReadOnlyList<OrderSummaryDto>>
{
    private readonly IOrderRepository _orders;

    public GetOrdersByUserQueryHandler(IOrderRepository orders) => _orders = orders;

    public async Task<IReadOnlyList<OrderSummaryDto>> Handle(
        GetOrdersByUserQuery query,
        CancellationToken ct)
    {
        var items = await _orders.GetByUserIdAsync(query.UserId, ct);

        return items
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.UserId,
                o.Status.ToString(),
                o.Total,
                o.CreatedAt))
            .ToList();
    }
}
