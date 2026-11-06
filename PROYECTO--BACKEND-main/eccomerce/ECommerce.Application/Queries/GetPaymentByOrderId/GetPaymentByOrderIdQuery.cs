using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Queries.GetPaymentByOrderId;

public record GetPaymentByOrderIdQuery(int OrderId) : IRequest<PaymentResponseDto?>;

