using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Data;

public class IdentityDataSeeder
{
    public const string AdminEmail = "admin@ecommerce.com";
    public const string AdminDefaultPassword = "Admin123!";

    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        ApplicationDbContext db,
        IPasswordHasher passwordHasher,
        ILogger<IdentityDataSeeder> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.Users.AnyAsync(u => u.Email == AdminEmail, ct))
            return;

        var admin = new User(
            AdminEmail,
            "Administrador",
            _passwordHasher.Hash(AdminDefaultPassword),
            "Admin");

        await _db.Users.AddAsync(admin, ct);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Usuario administrador creado: {Email} / contraseña por defecto: {Password}",
            AdminEmail,
            AdminDefaultPassword);
    }
}
