using Inventory.Application.Interfaces;
using Inventory.Application.Models;
using Inventory.Application.Warehouses;
using Inventory.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sgtin;

namespace Inventory.Application.Receipts;

public class ImportReceiptCommand : IRequest<ResponseBaseModel<List<string>>>
{
    public string InventoryCode { get; set; }
    public string InventoryLocation { get; set; }
    public DateTime? DateOfInventory { get; set; }
    public List<string>? Tags { get; set; }

    private sealed class
        ImportReceiptCommandHandler : IRequestHandler<ImportReceiptCommand, ResponseBaseModel<List<string>>>
    {
        private readonly IInventoryDbContext _db;
        private readonly ILogger<ImportReceiptCommandHandler> _log;
        private readonly IMediator _mediator;
        private readonly IDateTime _dateTime;

        public ImportReceiptCommandHandler(IInventoryDbContext db, ILogger<ImportReceiptCommandHandler> log,
            IMediator mediator, IDateTime dateTime)
        {
            _db = db;
            _log = log;
            _mediator = mediator;
            _dateTime = dateTime;
        }

        public async Task<ResponseBaseModel<List<string>>> Handle(ImportReceiptCommand request,
            CancellationToken cancellationToken)
        {
            List<string> errors = new();
            Guid warehouseId = Guid.Empty;
            if (string.IsNullOrEmpty(request.InventoryCode))
            {
                errors.Add("INVALID_INVENTORY_CODE");
            }
            else
            {
                warehouseId = await _db.Warehouses.Where(x => x.Code.ToLower().Equals(request.InventoryCode.ToLower()))
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (string.IsNullOrEmpty(request.InventoryLocation))
                {
                    return ResponseBaseModel<List<string>>.Failure(new[] { "INVALID_INVENTORY_LOCATION" });
                }

                if (warehouseId == Guid.Empty)
                {
                    var inventorySave = await _mediator.Send(new CreateWarehouseCommand()
                    {
                        Code = request.InventoryCode,
                        Location = request.InventoryLocation
                    }, cancellationToken);

                    if (!inventorySave.Succeess)
                    {
                        return ResponseBaseModel<List<string>>.Failure(inventorySave.Errors);
                    }

                    warehouseId = Guid.Parse(inventorySave.Result);
                }
            }

            if (!request.DateOfInventory.HasValue)
            {
                request.DateOfInventory = _dateTime.Now;
            }

            if (request.Tags is null || request.Tags.Count == 0)
            {
                errors.Add("TAGS_MISSING");
            }

            if (errors.Count > 0)
            {
                return ResponseBaseModel<List<string>>.Failure(errors.ToArray());
            }

            foreach (var tag in request.Tags)
            {
                SgtinTagInfo tagData;
                try
                {
                    tagData = SgtinProcessor.Decode(tag);
                }
                catch (ArgumentNullException ane)
                {
                    errors.Add($"{ane.Message}");
                    continue;
                }
                catch (ArgumentOutOfRangeException aore)
                {
                    errors.Add($"{aore.Message}, {tag}");
                    continue;
                }
                catch (NotSupportedException nse)
                {
                    errors.Add($"{nse.Message}, {tag}");
                    continue;
                }
                catch (FormatException fe)
                {
                    errors.Add($"{fe.Message}, {tag}");
                    continue;
                }
                catch (Exception e)
                {
                    errors.Add($"Cannot process tag, {tag}");
                    continue;
                }

                var productId = await _db.Products
                    .Where(x => x.ReferenceNumber == (long)tagData.ItemReference &&
                                x.Company.Prefix == (long)tagData.CompanyPrefix)
                    .Select(x => x.Id).FirstOrDefaultAsync(cancellationToken);

                if (productId == Guid.Empty)
                {
                    errors.Add($"Product or company not found: {tag}");
                    continue;
                }

                var inventoryItem = new WarehouseItem()
                {
                    Id = Guid.NewGuid(),
                    WarehouseId = warehouseId,
                    ProductId = productId,
                    Date = request.DateOfInventory.Value,
                    SerialNumber = (long)tagData.Serial
                };
                _db.WarehouseItems.Add(inventoryItem);
            }

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                return ResponseBaseModel<List<string>>.Succeeded(errors);
            }
            catch (Exception e)
            {
                _log.LogError("Unknown error occured while saving changes: {Message}, StackTrace: {StackTrace}",
                    e.Message, e.StackTrace);
                return ResponseBaseModel<List<string>>.Failure(new[] { "UNKNOWN_ERROR" });
            }
        }
    }
}