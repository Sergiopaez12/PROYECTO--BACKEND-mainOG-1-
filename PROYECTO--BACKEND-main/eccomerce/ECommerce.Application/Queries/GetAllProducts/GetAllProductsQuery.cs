using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Queries.GetAllProducts;

public record GetAllProductsQuery : IRequest<IReadOnlyList<ProductDto>>;

