using System.Net.Http.Headers;
using System.Net.Http.Json;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// CLIENTE HTTP TIPADO (IPaymentClient).
/// Implementa la comunicación inter-servicios desde ECommerce hacia PaymentService usando IHttpClientFactory
/// con reintentos exponenciales (2s, 4s, 8s) ante fallos transitorios.
/// </summary>
public class PaymentHttpClient : IPaymentClient
{
    private const int MaxRetries = 3;
    private readonly HttpClient _httpClient; // Cliente HTTP inyectado por IHttpClientFactory
    private readonly IHttpContextAccessor _httpContextAccessor; // Permite acceder a las cabeceras HTTP de la solicitud actual

    public PaymentHttpClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
    }

    private void PropagateJwtToken()
    {
        // 1. PROPAGACIÓN DE TOKEN JWT: Extraer la cabecera Authorization recibida en el ECommerce
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader["Bearer ".Length..].Trim();
            // Reenviar el Token Bearer en la llamada HTTP hacia PaymentService
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<PaymentResponseDto?> ProcessPaymentAsync(ProcessPaymentRequestDto request, CancellationToken ct = default)
    {
        PropagateJwtToken();

        for (int attempt = 1; ; attempt++)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/payments/process", request, ct);
                if ((int)response.StatusCode >= 500 && attempt <= MaxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
                    continue;
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<PaymentResponseDto>(cancellationToken: ct);
            }
            catch (HttpRequestException) when (attempt <= MaxRetries)
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
            }
        }
    }

    /// <summary>
    /// Consultar estado del pago por ID de orden desde el microservicio PaymentService.
    /// </summary>
    public async Task<PaymentResponseDto?> GetPaymentByOrderIdAsync(int orderId, CancellationToken ct = default)
    {
        PropagateJwtToken();
        return await _httpClient.GetFromJsonAsync<PaymentResponseDto>($"api/payments/order/{orderId}", ct);
    }
}
