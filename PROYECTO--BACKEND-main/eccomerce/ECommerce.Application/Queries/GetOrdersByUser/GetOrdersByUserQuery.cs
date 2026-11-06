using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Queries.GetOrdersByUser;

public record GetOrdersByUserQuery(int UserId) : IRequest<IReadOnlyList<OrderSummaryDto>>;

