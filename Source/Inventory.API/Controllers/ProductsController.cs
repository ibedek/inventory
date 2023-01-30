using Inventory.Application.Models;
using Inventory.Application.Products;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : BaseController
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponseBaseModel<ProductDto>>> GetProducrs(
        [FromQuery] GetProductsQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> InsertProduct(InsertProductCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost("bulk-insert")]
    public async Task<ActionResult<ResponseBaseModel<string>>> BulkInsert([FromForm] BulkInsertProductsCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
}