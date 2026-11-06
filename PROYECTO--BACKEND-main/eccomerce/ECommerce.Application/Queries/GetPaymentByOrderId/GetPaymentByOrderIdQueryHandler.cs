using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Queries.GetPaymentByOrderId;

public class GetPaymentByOrderIdQueryHandler : IRequestHandler<GetPaymentByOrderIdQuery, PaymentResponseDto?>
{
    private readonly IPaymentClient _paymentClient;

    public GetPaymentByOrderIdQueryHandler(IPaymentClient paymentClient)
    {
        _paymentClient = paymentClient;
    }

    public async Task<PaymentResponseDto?> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        return await _paymentClient.GetPaymentByOrderIdAsync(request.OrderId, cancellationToken);
    }
}

