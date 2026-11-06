using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
