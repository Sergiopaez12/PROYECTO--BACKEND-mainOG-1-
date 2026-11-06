namespace ECommerce.Application.DTOs;

public record OrderItemDto(int ProductId, decimal UnitPrice, int Quantity, decimal Subtotal);

public record OrderDto(
    int Id,
    int UserId,
    string Status,
    decimal Total,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDto> Items);

public record OrderSummaryDto(
    int Id,
    int UserId,
    string Status,
    decimal Total,
    DateTime CreatedAt);
