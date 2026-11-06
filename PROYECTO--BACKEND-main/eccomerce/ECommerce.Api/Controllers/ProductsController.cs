using ECommerce.Application.Commands.CreateProduct;
using ECommerce.Application.Commands.DeleteProduct;
using ECommerce.Application.Commands.UpdateProductPrice;
using ECommerce.Application.DTOs;
using ECommerce.Application.Queries.GetAllProducts;
using ECommerce.Application.Queries.GetProductById;
using ECommerce.Application.Queries.SearchProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>Query — listar todos los productos.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken ct)
    {
        var items = await _sender.Send(new GetAllProductsQuery(), ct);
        return Ok(items);
    }

    /// <summary>Query — obtener producto por id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken ct)
    {
        var product = await _sender.Send(new GetProductByIdQuery(id), ct);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>Query — buscar productos por nombre.</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Search([FromQuery] string term, CancellationToken ct)
    {
        try
        {
            var items = await _sender.Send(new SearchProductsQuery(term), ct);
            return Ok(items);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Command — crear producto (requiere rol Admin).</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken ct)
    {
        try
        {
            var command = new CreateProductCommand(
                request.Name,
                request.Description ?? string.Empty,
                request.Price,
                request.Stock,
                request.CategoryId);

            var created = await _sender.Send(command, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Command — actualizar precio (requiere rol Admin).</summary>
    [HttpPut("{id:int}/price")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePrice(
        int id,
        [FromBody] UpdatePriceRequest request,
        CancellationToken ct)
    {
        try
        {
            await _sender.Send(new UpdateProductPriceCommand(id, request.Price), ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Command — eliminar producto (requiere rol Admin).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            await _sender.Send(new DeleteProductCommand(id), ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    int CategoryId);

public record UpdatePriceRequest(decimal Price);
