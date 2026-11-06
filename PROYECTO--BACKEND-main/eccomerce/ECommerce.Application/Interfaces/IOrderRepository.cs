using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdWithItemsAsync(int id, CancellationToken ct = default);
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
}
