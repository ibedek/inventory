using Inventory.Application.Interfaces;
using Inventory.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Reports;

public class GetItemsByCompanyQuery : PaginatedRequestBaseModel, IRequest<PaginatedResponseBaseModel<ReportVm>>
{
    public string? CompanyName { get; set; }
    public long? CompanyPrefix { get; set; }
    public Guid? CompanyId { get; set; }

    private sealed class
        GetItemsByCompanyQueryHandler : IRequestHandler<GetItemsByCompanyQuery, PaginatedResponseBaseModel<ReportVm>>
    {
        private readonly IInventoryDbContext _db;


        public GetItemsByCompanyQueryHandler(IInventoryDbContext db)
        {
            _db = db;
        }

        public async Task<PaginatedResponseBaseModel<ReportVm>> Handle(GetItemsByCompanyQuery request,
            CancellationToken cancellationToken)
        {
            var query = _db.WarehouseItems
                .Include(x => x.Product.Company)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.CompanyName))
            {
                query = query.Where(x => x.Product.Company.Name.ToLower().Contains(request.CompanyName.ToLower()));
            }
            else if (request.CompanyPrefix.HasValue)
            {
                query = query.Where(x => x.Product.Company.Prefix == request.CompanyPrefix.Value);
            }
            else if (request.CompanyId.HasValue && request.CompanyId != Guid.Empty)
            {
                query = query.Where(x => x.Product.CompanyId == request.CompanyId);
            }

            var groupedQuery = query.GroupBy(x => new
                {
                    x.ProductId,
                    x.Product.CompanyId,
                    x.Product.ReferenceNumber,
                    ProductName = x.Product.Name,
                    x.Product.Company.Prefix,
                    CompanyName = x.Product.Company.Name,
                })
                .Select(x => new ReportVm()
                {
                    ProductName = x.Key.ProductName,
                    ProductReference = x.Key.ReferenceNumber,
                    CompanyName = x.Key.CompanyName,
                    CompanyPrefix = x.Key.Prefix,
                    TotalCount = x.Count()
                });

            var totalCount = await groupedQuery.CountAsync(cancellationToken);
            var results = await groupedQuery
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new()
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Results = results,
                TotalResultCount = totalCount
            };
        }
    }
}