using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Queries.GetAllProducts;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _products;

    public GetAllProductsQueryHandler(IProductRepository products) => _products = products;

    public async Task<IReadOnlyList<ProductDto>> Handle(
        GetAllProductsQuery query,
        CancellationToken ct)
    {
        var items = await _products.GetAllAsync(ct);

        return items
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Stock,
                p.CategoryId,
                p.CreatedAt))
            .ToList();
    }
}
