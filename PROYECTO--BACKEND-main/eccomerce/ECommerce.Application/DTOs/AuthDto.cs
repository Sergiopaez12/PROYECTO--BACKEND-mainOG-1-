namespace ECommerce.Application.DTOs;

public record AuthDto(int UserId, string Email, string Name, string Role, string Token);
