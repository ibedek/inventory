using Inventory.Application.Companies;
using Inventory.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : BaseController
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponseBaseModel<CompanyDto>>> GetCompanies([FromQuery] GetCompaniesQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpPut]
    public async Task<ActionResult<CompanyDto>> UpdateCompany(UpsertCompanyCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost]
    public async Task<ActionResult<CompanyDto>> CreateCompany(UpsertCompanyCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
}