using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Queries.SearchProducts;

public record SearchProductsQuery(string Term) : IRequest<IReadOnlyList<ProductDto>>;

