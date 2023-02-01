using Inventory.Application.Interfaces;
using Inventory.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Reports;

public class GetItemsCountPerDayQuery : PaginatedRequestBaseModel, IRequest<PaginatedResponseBaseModel<ReportVm>>
{
    public DateTime? Date { get; set; }

    private sealed class GetItemsCountPerDayQueryHandler : IRequestHandler<GetItemsCountPerDayQuery, PaginatedResponseBaseModel<ReportVm>>
    {
        private readonly IInventoryDbContext _db;

        public GetItemsCountPerDayQueryHandler(IInventoryDbContext db)
        {
            _db = db;
        }

        public async Task<PaginatedResponseBaseModel<ReportVm>> Handle(GetItemsCountPerDayQuery request, CancellationToken cancellationToken)
        {
            var query = _db.WarehouseItems
                .Include(x => x.Warehouse)
                .Include(x => x.Product.Company)
                .AsQueryable();

            if (request.Date.HasValue)
            {
                // DB optimization
                var startDate = new DateTime(request.Date.Value.Year, request.Date.Value.Month, request.Date.Value.Day);
                var endDate = startDate.AddDays(1);

                query = query.Where(x => x.Date >= startDate && x.Date < endDate);
            }

            var groupedQuery = query.GroupBy(x => new
            {
                x.ProductId,
                x.Date.Date,
                ProductName = x.Product.Name,
                x.Product.ReferenceNumber,
                CompanyName = x.Product.Company.Name,
                x.Product.Company.Prefix
            })
                .Select(x => new ReportVm()
                {
                    ProductReference = x.Key.ReferenceNumber,
                    ProductName = x.Key.ProductName,
                    CompanyName = x.Key.CompanyName,
                    CompanyPrefix = x.Key.Prefix,
                    DateOfInventory = x.Key.Date,
                    TotalCount = x.Count()
                })
                ;

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