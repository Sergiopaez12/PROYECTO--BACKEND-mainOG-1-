using MediatR;

namespace ECommerce.Application.Commands.UpdateProductPrice;

public record UpdateProductPriceCommand(int ProductId, decimal Price) : IRequest;

