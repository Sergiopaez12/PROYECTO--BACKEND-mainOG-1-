using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using MediatR;

namespace ECommerce.Application.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly ICategoryRepository _categories;

    public GetAllCategoriesQueryHandler(ICategoryRepository categories) => _categories = categories;

    public async Task<IReadOnlyList<CategoryDto>> Handle(
        GetAllCategoriesQuery query,
        CancellationToken ct)
    {
        var items = await _categories.GetAllAsync(ct);
        return items.Select(c => new CategoryDto(c.Id, c.Name)).ToList();
    }
}
