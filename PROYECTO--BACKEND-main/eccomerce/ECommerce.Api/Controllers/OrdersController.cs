using ECommerce.Application.Commands.CreateOrder;
using ECommerce.Application.DTOs;
using ECommerce.Application.Queries.GetOrderById;
using ECommerce.Application.Queries.GetOrdersByUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// ENDPOINT 1: Query — Obtener pedido por su Id único.
    /// Ruta: GET /api/orders/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(int id, CancellationToken ct)
    {
        var order = await _sender.Send(new GetOrderByIdQuery(id), ct);
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>
    /// ENDPOINT 2: Query — Listar todos los pedidos realizados por un usuario.
    /// Ruta: GET /api/orders/user/{userId}
    /// </summary>
    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> GetByUser(int userId, CancellationToken ct)
    {
        var orders = await _sender.Send(new GetOrdersByUserQuery(userId), ct);
        return Ok(orders);
    }

    /// <summary>
    /// ENDPOINT 3: Command — Crear un nuevo pedido de compra y procesar su pago en PaymentService.
    /// Ruta: POST /api/orders
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        try
        {
            var command = new CreateOrderCommand(
                request.UserId,
                request.Items
                    .Select(i => new CreateOrderLine(i.ProductId, i.Quantity))
                    .ToList());

            var created = await _sender.Send(command, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (HttpRequestException ex)
        {
            // Ocurre si PaymentService está caído o no responde la conexión HTTP
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = "El servicio de pagos (PaymentService) no se encuentra disponible actualmente.",
                orderStatus = "Pending",
                details = ex.Message,
                actionRequired = "El pedido permanece guardado en estado 'Pending' (Pendiente). Podrá reintentar el pago cuando el servicio se restablezca."
            });
        }
        catch (TaskCanceledException)
        {
            // Ocurre si PaymentService tarda más tiempo del configurado (Timeout)
            return StatusCode(StatusCodes.Status504GatewayTimeout, new
            {
                message = "El servicio de pagos tardó demasiado tiempo en responder (Tiempo de espera agotado / Timeout).",
                orderStatus = "Pending",
                actionRequired = "El pedido permanece en estado 'Pending' (Pendiente). Por favor verifique el estado del pago antes de volver a intentar."
            });
        }
    }
}

// DTO de entrada para la creación de la orden: contiene el Id del usuario y la lista de ítems a comprar
public record CreateOrderRequest(int UserId, List<OrderLineRequest> Items);

// DTO de entrada para cada línea del pedido: ID del producto y la cantidad solicitada
public record OrderLineRequest(int ProductId, int Quantity);

