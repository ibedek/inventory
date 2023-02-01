using Inventory.Application.Models;
using Inventory.Application.Reports;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : BaseController
{
    [HttpGet]
    [Route("items-by-warehouse")]
    public async Task<ActionResult<PaginatedResponseBaseModel<ReportVm>>> GetItemsCountByWarehouse(
        [FromQuery] GetItemsCountByWarehouseQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet]
    [Route("items-by-day")]
    public async Task<ActionResult<PaginatedResponseBaseModel<ReportVm>>> GetItemsCountPerDay(
        [FromQuery] GetItemsCountPerDayQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet]
    [Route("items-by-company")]
    public async Task<ActionResult<PaginatedResponseBaseModel<ReportVm>>> GetItemsCountByCompany(
        [FromQuery] GetItemsByCompanyQuery query)
    {
        return Ok(await Mediator.Send(query));
    }
}