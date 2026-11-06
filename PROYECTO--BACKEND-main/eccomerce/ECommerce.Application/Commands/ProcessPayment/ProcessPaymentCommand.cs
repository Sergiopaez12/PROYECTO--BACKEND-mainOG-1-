using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Commands.ProcessPayment;

public record ProcessPaymentCommand(
    int OrderId,
    int UserId,
    decimal Amount,
    string PaymentMethod,
    string CardNumber,
    string ExpiryDate,
    string Cvv
) : IRequest<PaymentResponseDto?>;

