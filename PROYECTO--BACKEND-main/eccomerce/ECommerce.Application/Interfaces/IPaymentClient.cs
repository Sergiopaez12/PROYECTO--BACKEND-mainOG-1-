using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces;

public interface IPaymentClient
{
    Task<PaymentResponseDto?> ProcessPaymentAsync(ProcessPaymentRequestDto request, CancellationToken ct = default);
    Task<PaymentResponseDto?> GetPaymentByOrderIdAsync(int orderId, CancellationToken ct = default);
}

