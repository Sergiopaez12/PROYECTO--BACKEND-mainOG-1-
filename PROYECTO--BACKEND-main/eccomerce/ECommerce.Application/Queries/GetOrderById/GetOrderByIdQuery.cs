using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Queries.GetOrderById;

public record GetOrderByIdQuery(int Id) : IRequest<OrderDto?>;

