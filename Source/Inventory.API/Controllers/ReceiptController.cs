using Inventory.Application.Models;
using Inventory.Application.Receipts;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptController : BaseController
{
    [HttpPost]
    public async Task<ActionResult<ResponseBaseModel<List<string>>>> ImportReceipt(ImportReceiptCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
}