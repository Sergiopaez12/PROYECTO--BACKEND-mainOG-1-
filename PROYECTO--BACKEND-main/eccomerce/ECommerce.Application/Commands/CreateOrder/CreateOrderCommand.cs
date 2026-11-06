using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Commands.CreateOrder;

public record CreateOrderLine(int ProductId, int Quantity);

public record CreateOrderCommand(int UserId, IReadOnlyList<CreateOrderLine> Items) : IRequest<OrderDto>;



