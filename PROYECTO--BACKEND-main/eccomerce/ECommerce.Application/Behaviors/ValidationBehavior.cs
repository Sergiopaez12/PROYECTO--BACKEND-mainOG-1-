
using FluentValidation;
using MediatR;

namespace ECommerce.Application.Behaviors;

/// <summary>
/// Pipeline Behavior de MediatR que intercepta automáticamente cada Request (Command/Query).
/// Si existen validadores registrados con FluentValidation para ese tipo de solicitud,
/// los ejecuta antes de llegar al Handler correspondiente.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Si no hay validadores registrados para este comando/query, continuar normalmente al Handler
        if (!_validators.Any())
            return await next();

        // Ejecutar todos los validadores registrados para este request
        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // Si se encontraron errores de validación, lanzar ValidationException para que el middleware lo capture
        if (failures.Count != 0)
            throw new ValidationException(failures);

        // Sin errores -> continuar la ejecución hacia el Handler del caso de uso
        return await next();
    }
}