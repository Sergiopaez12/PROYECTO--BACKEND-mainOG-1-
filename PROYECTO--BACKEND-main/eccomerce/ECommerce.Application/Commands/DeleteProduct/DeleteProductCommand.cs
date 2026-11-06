using MediatR;

namespace ECommerce.Application.Commands.DeleteProduct;

public record DeleteProductCommand(int ProductId) : IRequest;

