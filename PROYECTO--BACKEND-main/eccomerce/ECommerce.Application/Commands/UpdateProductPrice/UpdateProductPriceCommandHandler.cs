using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Commands.UpdateProductPrice;

public class UpdateProductPriceCommandHandler : IRequestHandler<UpdateProductPriceCommand>
{
    private readonly IProductRepository _products;

    public UpdateProductPriceCommandHandler(IProductRepository products) => _products = products;

    public async Task Handle(UpdateProductPriceCommand command, CancellationToken ct)
    {
        var product = await _products.GetByIdAsync(command.ProductId, ct)
            ?? throw new KeyNotFoundException("Producto no encontrado.");

        product.UpdatePrice(command.Price);
        await _products.UpdateAsync(product, ct);
    }
}

