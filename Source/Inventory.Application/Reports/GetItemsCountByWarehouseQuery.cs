using System.Formats.Asn1;
using Inventory.Application.Interfaces;
using Inventory.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Reports;

public class GetItemsCountByWarehouseQuery : PaginatedRequestBaseModel, IRequest<PaginatedResponseBaseModel<ReportVm>>
{
    public string? WarehouseCode { get; set; }
    public Guid WarehouseId { get; set; } = default!;
    
    private sealed class GetItemsCountByWarehouseQueryHandler : IRequestHandler<GetItemsCountByWarehouseQuery, PaginatedResponseBaseModel<ReportVm>>
    {
        private readonly IInventoryDbContext _db;

        public GetItemsCountByWarehouseQueryHandler(IInventoryDbContext db)
        {
            _db = db;
        }

        public async Task<PaginatedResponseBaseModel<ReportVm>> Handle(GetItemsCountByWarehouseQuery request, CancellationToken cancellationToken)
        {
            var query = _db.WarehouseItems
                .Include(x => x.Warehouse)
                .Include(x => x.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.WarehouseCode))
            {
                query = query.Where(x => x.Warehouse.Code.ToLower().Equals(request.WarehouseCode.ToLower()));
            } else if (request.WarehouseId != Guid.Empty)
            {
                query = query.Where(x => x.Warehouse.Id == request.WarehouseId);
            }

            var groupedQuery = query
                .GroupBy(x => new
                {
                    ProductId = x.ProductId, 
                    ProductName = x.Product.Name, 
                    ProductReference = x.Product.ReferenceNumber, 
                    CompanyName = x.Product.Company.Name, 
                    CompanyPrefix = x.Product.Company.Prefix    
                })
                .Select(x => new ReportVm()
                {
                    ProductName = x.Key.ProductName,
                    ProductReference = x.Key.ProductReference,
                    CompanyName = x.Key.CompanyName,
                    CompanyPrefix = x.Key.CompanyPrefix,
                    TotalCount = x.Count()
                });
            var totalCount = await groupedQuery.CountAsync(cancellationToken);
            var result = await groupedQuery
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new()
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Results = result,
                TotalResultCount = totalCount
            };
        }
    }
}