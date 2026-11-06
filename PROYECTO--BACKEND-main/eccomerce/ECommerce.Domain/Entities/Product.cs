using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.Entities;

public class Product
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public int CategoryId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Product() { }

    public Product(string name, string description, decimal price, int stock, int categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del producto es obligatorio.");
        if (price < 0)
            throw new InvalidMoneyException("El precio no puede ser negativo.");
        if (stock < 0)
            throw new DomainException("El stock no puede ser negativo.");

        Name = name;
        Description = description;
        Price = price;
        Stock = stock;
        CategoryId = categoryId;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new InvalidMoneyException("El nuevo precio no puede ser negativo.");
        Price = newPrice;
    }

    public void ReduceStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad a reducir debe ser mayor a cero.");
        if (quantity > Stock)
            throw new InsufficientStockException(Id, quantity, Stock);
        Stock -= quantity;
    }

    public void RestoreStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("La cantidad a reponer debe ser mayor a cero.");
        Stock += quantity;
    }
}
