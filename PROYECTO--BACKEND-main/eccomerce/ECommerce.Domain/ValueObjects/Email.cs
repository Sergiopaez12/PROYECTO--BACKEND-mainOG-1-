using System.Text.RegularExpressions;
using ECommerce.Domain.Exceptions;

namespace ECommerce.Domain.ValueObjects;

public record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; init; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El correo electrónico es obligatorio.");

        var trimmed = value.Trim();
        if (!EmailRegex.IsMatch(trimmed))
            throw new DomainException($"El formato del correo electrónico '{value}' es inválido.");

        Value = trimmed.ToLowerInvariant();
    }

    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string value) => new(value);
}
