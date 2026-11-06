using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _ctx;

    public CategoryRepository(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);
}
