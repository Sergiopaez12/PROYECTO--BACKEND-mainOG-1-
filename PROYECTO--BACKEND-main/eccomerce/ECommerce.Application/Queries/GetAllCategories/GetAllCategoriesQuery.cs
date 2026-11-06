using ECommerce.Application.DTOs;
using MediatR;

namespace ECommerce.Application.Queries.GetAllCategories;

public record GetAllCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;

