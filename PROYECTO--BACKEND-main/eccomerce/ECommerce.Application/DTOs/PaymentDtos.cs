namespace ECommerce.Application.DTOs;

public record ProcessPaymentRequestDto(
    int OrderId,
    int UserId,
    decimal Amount,
    string PaymentMethod,
    string CardNumber,
    string ExpiryDate,
    string Cvv
);

public record PaymentResponseDto(
    int Id,
    int OrderId,
    int UserId,
    decimal Amount,
    string PaymentMethod,
    string Status,
    string? TransactionId,
    DateTime CreatedAt
);

