using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, PaymentResponseDto?>
{
    private readonly IPaymentClient _paymentClient;

    public ProcessPaymentCommandHandler(IPaymentClient paymentClient)
    {
        _paymentClient = paymentClient;
    }

    public async Task<PaymentResponseDto?> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var dto = new ProcessPaymentRequestDto(
            request.OrderId,
            request.UserId,
            request.Amount,
            request.PaymentMethod,
            request.CardNumber,
            request.ExpiryDate,
            request.Cvv
        );

        return await _paymentClient.ProcessPaymentAsync(dto, cancellationToken);
    }
}

