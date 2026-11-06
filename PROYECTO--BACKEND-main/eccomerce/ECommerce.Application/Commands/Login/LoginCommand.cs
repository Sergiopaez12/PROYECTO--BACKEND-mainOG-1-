using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthDto>;

