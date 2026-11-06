using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Queries.GetProductById;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;

