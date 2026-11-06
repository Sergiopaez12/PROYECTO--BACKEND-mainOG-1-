using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";

    public Money(decimal amount, string currency = "USD")
    {
        if (amount < 0)
            throw new InvalidMoneyException("El monto no puede ser negativo.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidMoneyException("La moneda es obligatoria.");

        Amount = decimal.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency = "USD") => new(0m, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(int factor)
    {
        if (factor < 0)
            throw new InvalidMoneyException("El factor de multiplicación no puede ser negativo.");
        return new Money(Amount * factor, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidMoneyException($"No se pueden operar monedas distintas: {Currency} y {other.Currency}.");
    }

    public static implicit operator decimal(Money money) => money.Amount;
    public static implicit operator Money(decimal amount) => new(amount);
}
