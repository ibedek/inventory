using Inventory.Application.Interfaces;
using Inventory.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Products;

public class BulkInsertProductsCommand : IRequest<ResponseBaseModel<string>>
{
    public IFormFile File { get; set; }
    public bool HasHeaders { get; set; } = true;
    public string ColumnSeparator { get; set; } = ";";

    private sealed class
        BulkInsertProductsCommandHandler : IRequestHandler<BulkInsertProductsCommand, ResponseBaseModel<string>>
    {
        private readonly IInventoryDbContext _db;
        private readonly IDateTime _dateTime;
        private readonly ICurrentUser _currentUser;
        private readonly ILogger<BulkInsertProductsCommandHandler> _logger;

        public BulkInsertProductsCommandHandler(IInventoryDbContext db, IDateTime dateTime, ICurrentUser currentUser,
            ILogger<BulkInsertProductsCommandHandler> logger)
        {
            _db = db;
            _dateTime = dateTime;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<ResponseBaseModel<string>> Handle(BulkInsertProductsCommand request,
            CancellationToken cancellationToken)
        {
            using MemoryStream ms = new();
            request.File.CopyTo(ms);
            var content = System.Text.Encoding.UTF8.GetString(ms.ToArray());

            var fileRows = content.Split('\n');
            for (int i = request.HasHeaders ? 1 : 0; i < fileRows.Length; i++)
            {
                var columns = fileRows[i].Split(request.ColumnSeparator);
                if (!long.TryParse(columns[0], out long companyPrefix))
                {
                    continue;
                }

                if (!long.TryParse(columns[2], out long productReference))
                {
                    continue;
                }

                Guid? companyId = await _db.Companies
                    .Where(x => x.Prefix == companyPrefix)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (companyId == Guid.Empty)
                {
                    companyId = Guid.NewGuid();
                    _db.Companies.Add(new(_dateTime.Now, _currentUser.Username)
                    {
                        Id = companyId.Value,
                        Prefix = companyPrefix,
                        Name = columns[1]
                    });
                }

                Guid? productId = await _db.Products
                    .Where(x => x.ReferenceNumber == productReference && x.CompanyId == companyId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (productId == Guid.Empty)
                {
                    _db.Products.Add(new(_dateTime.Now, _currentUser.Username)
                    {
                        Id = Guid.NewGuid(),
                        ReferenceNumber = productReference,
                        Name = columns[3],
                        CompanyId = companyId.Value
                    });
                }
            }

            try
            {
                int totalItems = await _db.SaveChangesAsync(cancellationToken);
                return ResponseBaseModel<string>.Succeeded($"Inserted {totalItems} rows");
            }
            catch (Exception e)
            {
                _logger.LogError(
                    "Unknown error occured while batch saving changes: {Message}, StackTrace: {StackTrace}", e.Message,
                    e.StackTrace);
                return ResponseBaseModel<string>.Failure(new string[] { "UNKNOWN_ERROR" });
            }
        }
    }
}