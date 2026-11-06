using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using MediatR;

namespace ECommerce.Application.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthDto>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthDto> Handle(RegisterCommand command, CancellationToken ct)
    {
        if (await _users.ExistsByEmailAsync(command.Email, ct))
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        var hash = _passwordHasher.Hash(command.Password);
        var user = new User(command.Email, command.Name, hash);
        await _users.AddAsync(user, ct);

        return BuildAuthDto(user);
    }

    private AuthDto BuildAuthDto(User user)
    {
        var token = _jwtTokenGenerator.GenerateToken(user);
        return new AuthDto(user.Id, user.Email, user.Name, user.Role, token);
    }
}
