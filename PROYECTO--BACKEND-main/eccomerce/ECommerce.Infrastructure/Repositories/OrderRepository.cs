using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _ctx;

    public OrderRepository(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<Order?> GetByIdWithItemsAsync(int id, CancellationToken ct = default)
        => await _ctx.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default)
        => await _ctx.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await using var transaction = await _ctx.Database.BeginTransactionAsync(ct);
        try
        {
            await _ctx.Orders.AddAsync(order, ct);
            await _ctx.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _ctx.Orders.Update(order);
        await _ctx.SaveChangesAsync(ct);
    }
}
