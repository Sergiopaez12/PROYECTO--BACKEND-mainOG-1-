using System.Net;
using System.Text.Json;
using ECommerce.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Middleware;

/// <summary>
/// Manejador global de excepciones en .NET 8 (IExceptionHandler).
/// Captura errores no controlados, validaciones de FluentValidation y excepciones de dominio,
/// transformándolos en respuestas JSON con códigos HTTP adecuados.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ocurrió una excepción no controlada: {Message}", exception.Message);

        var (statusCode, response) = exception switch
        {
            // Errores de validación de FluentValidation (desde ValidationBehavior)
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                new
                {
                    status = StatusCodes.Status400BadRequest,
                    title = "Error de validación",
                    errors = validationEx.Errors.Select(e => new
                    {
                        property = e.PropertyName,
                        error = e.ErrorMessage
                    })
                }),

            // Errores de reglas del Dominio (ej. stock insuficiente, estado de orden inválido)
            DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                (object)new { status = StatusCodes.Status400BadRequest, title = "Error de regla de negocio", message = domainEx.Message }),

            // Recurso no encontrado
            KeyNotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                new { status = StatusCodes.Status404NotFound, title = "No encontrado", message = notFoundEx.Message }),

            // Credenciales incorrectas
            UnauthorizedAccessException unauthorizedEx => (
                StatusCodes.Status401Unauthorized,
                new { status = StatusCodes.Status401Unauthorized, title = "No autorizado", message = unauthorizedEx.Message }),

            // Argumentos inválidos
            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                new { status = StatusCodes.Status400BadRequest, title = "Solicitud incorrecta", message = argEx.Message }),

            // Error de servidor general
            _ => (
                StatusCodes.Status500InternalServerError,
                new { status = StatusCodes.Status500InternalServerError, title = "Error interno del servidor", message = "Ocurrió un error inesperado." })
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(response),
            cancellationToken);

        return true;
    }
}
