using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthDto> Handle(LoginCommand command, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(command.Email, ct)
            ?? throw new UnauthorizedAccessException("Email o contraseña incorrectos.");

        if (!_passwordHasher.Verify(command.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email o contraseña incorrectos.");

        var token = _jwtTokenGenerator.GenerateToken(user);
        return new AuthDto(user.Id, user.Email, user.Name, user.Role, token);
    }
}
