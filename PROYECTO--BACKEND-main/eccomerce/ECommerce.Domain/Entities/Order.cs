using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Posibles estados en el ciclo de vida de una orden de compra.
/// </summary>
public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>
/// </summary>
public class Order
{
    // Propiedades con setters privados para garantizar el encapsulamiento (DDD)
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Total { get; private set; }

    // Colección privada editable e interfaz pública de solo lectura para evitar modificaciones directas desde fuera
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Constructor privado requerido por Entity Framework Core para la hidratación desde la BD.
    /// </summary>
    private Order() { }

    /// <summary>
    /// Crea una nueva orden asociándola a un usuario específico y estableciendo el estado inicial a Pendiente.
    /// </summary>
    /// <param name="userId">Identificador único del usuario comprador.</param>
    public Order(int userId)
    {
        // Regla de negocio: No se puede crear una orden sin un usuario válido
        if (userId <= 0)
            throw new DomainException("UserId es obligatorio para crear un pedido.");

        UserId = userId;
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        Total = 0;
    }

    /// <summary>
    /// Agrega un producto y su cantidad a la orden actual.
    /// Contiene la regla de validación de cantidad, verificación de estado de la orden,
    /// reducción de stock del producto y recalculo del total acumulado.
    /// </summary>
    /// <param name="product">Instancia del producto a comprar.</param>
    /// <param name="quantity">Cantidad de unidades a solicitar.</param>
    public void AddItem(Product product, int quantity)
    {
        if (product is null)
            throw new ArgumentNullException(nameof(product));
        if (quantity <= 0)
            throw new DomainException("La cantidad debe ser mayor a cero.");

        // Regla de negocio: Solo se pueden agregar ítems si la orden está en estado Pendiente
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException("Solo se pueden agregar ítems a órdenes en estado pendiente.");

        // Reducir stock mediante la entidad Producto (regla de negocio encapsulada en Product)
        product.ReduceStock(quantity);

        var item = new OrderItem(Id, product.Id, product.Price, quantity);
        _items.Add(item);

        // Recalcular el total acumulado de la orden
        Total += item.Subtotal;
    }

    /// <summary>
    /// Confirma la orden de compra.
    /// Valida que la orden esté en estado Pendiente y que posea al menos un ítem agregado.
    /// </summary>
    public void Confirm()
    {
        // Regla de negocio: Solo órdenes pendientes pueden ser confirmadas
        if (Status != OrderStatus.Pending)
            throw new InvalidOrderStateException("Solo se pueden confirmar órdenes pendientes.");

        // Regla de negocio: No se puede confirmar una orden vacía
        if (_items.Count == 0)
            throw new InvalidOrderStateException("No se puede confirmar una orden sin ítems.");

        Status = OrderStatus.Confirmed;
    }

    /// <summary>
    /// Cancela la orden de compra.
    /// Valida que la orden no haya sido despachada ni entregada previamente.
    /// </summary>
    public void Cancel()
    {
        // Regla de negocio: Una orden enviada o entregada no se puede cancelar
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new InvalidOrderStateException("No se puede cancelar una orden que ya fue enviada o entregada.");

        Status = OrderStatus.Cancelled;
    }
}
