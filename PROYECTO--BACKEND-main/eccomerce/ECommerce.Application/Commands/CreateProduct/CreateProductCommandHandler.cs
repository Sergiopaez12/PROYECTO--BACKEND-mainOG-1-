using ECommerce.Application.Constants;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _products;

    public CreateProductCommandHandler(IProductRepository products) => _products = products;

    public async Task<ProductDto> Handle(CreateProductCommand command, CancellationToken ct)
    {
        var categoryId = command.CategoryId <= 0
            ? SeedCategoryIds.Electronica
            : command.CategoryId;

        var product = new Product(
            command.Name,
            command.Description,
            command.Price,
            command.Stock,
            categoryId);

        await _products.AddAsync(product, ct);

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
