using Inventory.Application.Interfaces;
using Inventory.Application.Models;
using Inventory.Core.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Warehouses;

public class CreateWarehouseCommand : IRequest<ResponseBaseModel<string>>
{
    public string Code { get; set; }
    public string Location { get; set; }

    private sealed class
        CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, ResponseBaseModel<string>>
    {
        private readonly IInventoryDbContext _db;
        private readonly ILogger<CreateWarehouseCommandHandler> _log;
        private readonly ICurrentUser _currentUser;
        private readonly IDateTime _dateTime;

        public CreateWarehouseCommandHandler(IInventoryDbContext db, ILogger<CreateWarehouseCommandHandler> log,
            ICurrentUser currentUser, IDateTime dateTime)
        {
            _db = db;
            _log = log;
            _currentUser = currentUser;
            _dateTime = dateTime;
        }

        public async Task<ResponseBaseModel<string>> Handle(CreateWarehouseCommand request,
            CancellationToken cancellationToken)
        {
            var warehouse = new Warehouse()
            {
                Id = Guid.NewGuid(),
                Code = request.Code,
                Location = request.Location,
                CreatedAt = _dateTime.Now,
                CreatedBy = _currentUser.Username
            };
            _db.Warehouses.Add(warehouse);

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                return ResponseBaseModel<string>.Succeeded(warehouse.Id.ToString());
            }
            catch (Exception e)
            {
                _log.LogError("Unknown error occured while saving warehouse: {Message}, StackTrace: {StackTrace}",
                    e.Message, e.StackTrace);
                return ResponseBaseModel<string>.Failure(new string[] { "UNKNOWN_ERROR" });
            }
        }
    }
}