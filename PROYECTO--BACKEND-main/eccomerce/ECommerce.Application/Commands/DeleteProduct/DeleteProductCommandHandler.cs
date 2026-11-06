using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IProductRepository _products;

    public DeleteProductCommandHandler(IProductRepository products) => _products = products;

    public async Task Handle(DeleteProductCommand command, CancellationToken ct)
    {
        if (!await _products.ExistsAsync(command.ProductId, ct))
            throw new KeyNotFoundException("Producto no encontrado.");

        await _products.DeleteAsync(command.ProductId, ct);
    }
}

