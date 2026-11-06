namespace ECommerce.Domain.Exceptions;

public class InsufficientStockException : DomainException
{
    public int ProductId { get; }
    public int RequestedQuantity { get; }
    public int AvailableStock { get; }

    public InsufficientStockException(int productId, int requested, int available)
        : base($"Stock insuficiente para el producto '{productId}'. Solicitado: {requested}, Disponible: {available}.")
    {
        ProductId = productId;
        RequestedQuantity = requested;
        AvailableStock = available;
    }
}

public class InvalidMoneyException : DomainException
{
    public InvalidMoneyException(string message) : base(message) { }
}

public class InvalidOrderStateException : DomainException
{
    public InvalidOrderStateException(string message) : base(message) { }
}
