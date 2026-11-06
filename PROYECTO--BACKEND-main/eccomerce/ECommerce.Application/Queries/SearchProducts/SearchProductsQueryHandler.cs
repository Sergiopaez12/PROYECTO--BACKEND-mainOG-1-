using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Queries.SearchProducts;

public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _products;

    public SearchProductsQueryHandler(IProductRepository products) => _products = products;

    public async Task<IReadOnlyList<ProductDto>> Handle(
        SearchProductsQuery query,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.Term))
            throw new ArgumentException("El término de búsqueda es obligatorio.");

        var items = await _products.SearchByNameAsync(query.Term, ct);

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
