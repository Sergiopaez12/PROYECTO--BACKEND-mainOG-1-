using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Queries.GetProductById;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _products;

    public GetProductByIdQueryHandler(IProductRepository products) => _products = products;

    public async Task<ProductDto?> Handle(GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await _products.GetByIdAsync(query.Id, ct);
        if (product is null)
            return null;

        return new ProductDto(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Stock,
            product.CategoryId,
            product.CreatedAt);
    }
}
