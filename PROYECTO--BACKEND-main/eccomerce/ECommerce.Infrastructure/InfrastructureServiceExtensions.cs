using System.Text;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Auth;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IdentityDataSeeder>();
        services.AddHttpContextAccessor();

        var paymentServiceUrl = configuration.GetSection("PaymentService:BaseUrl").Value
            ?? throw new InvalidOperationException("La configuración 'PaymentService:BaseUrl' es obligatoria en appsettings.json.");

        // Cliente HTTP tipado hacia PaymentService con Timeout de 10s y reintentos exponenciales
        services.AddHttpClient<IPaymentClient, Services.PaymentHttpClient>(client =>
        {
            client.BaseAddress = new Uri(paymentServiceUrl.EndsWith('/') ? paymentServiceUrl : paymentServiceUrl + "/");
            client.Timeout = TimeSpan.FromSeconds(10); // Timeout máximo de solicitud
        });

        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Configuración JWT no encontrada en appsettings.json.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))
                };

                // Personalizar la respuesta del error 403 Forbidden cuando el rol no es Admin
                options.Events = new JwtBearerEvents
                {
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        var jsonResponse = System.Text.Json.JsonSerializer.Serialize(new
                        {
                            message = "No estás en el rol de admin"
                        });
                        return context.Response.WriteAsync(jsonResponse);
                    }
                };
            });

        services.AddExceptionHandler<Middleware.GlobalExceptionHandler>();
        services.AddProblemDetails();
        services.AddAuthorization();

        return services;
    }
}
