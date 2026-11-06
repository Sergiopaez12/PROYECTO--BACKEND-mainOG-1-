using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Commands.Register;

public record RegisterCommand(string Email, string Name, string Password) : IRequest<AuthDto>;

