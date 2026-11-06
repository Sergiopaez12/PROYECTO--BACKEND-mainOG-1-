using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);
}
