using Inventory.Application.Interfaces;
using Inventory.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Products;

public class GetProductsQuery : PaginatedRequestBaseModel, IRequest<PaginatedResponseBaseModel<ProductDto>>
{
    public string? ProductName { get; set; }
    public long? ProductReference { get; set; }
    public string? CompanyName { get; set; }
    public long? CompanyPrefix { get; set; }

    private sealed class
        GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PaginatedResponseBaseModel<ProductDto>>
    {
        private readonly IInventoryDbContext _db;

        public GetProductsQueryHandler(IInventoryDbContext db)
        {
            _db = db;
        }

        public async Task<PaginatedResponseBaseModel<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Products
                .AsNoTracking()
                .Include(x => x.Company)
                .AsQueryable();
            
            if (!string.IsNullOrEmpty(request.ProductName))
            {
                query = query.Where(x => x.Name.ToLower().Contains(request.ProductName.ToLower()));
            }

            if (request.ProductReference.HasValue)
            {
                query = query.Where(x => x.ReferenceNumber == request.ProductReference.Value);
            }

            if (!string.IsNullOrEmpty(request.CompanyName))
            {
                query = query.Where(x => x.Company.Name.ToLower().Contains(request.CompanyName.ToLower()));
            }

            if (request.CompanyPrefix.HasValue)
            {
                query = query.Where(x => x.Company.Prefix == request.CompanyPrefix.Value);
            }

            var count = await query.CountAsync(cancellationToken);
            var results = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new ProductDto()
                {
                    Id = x.Id,
                    CompanyName = x.Company.Name,
                    CompanyPrefix = x.Company.Prefix,
                    Name = x.Name,
                    ItemReference = x.ReferenceNumber
                })
                .ToListAsync(cancellationToken);

            return new()
            {
                Page = request.Page,
                PageSize = request.PageSize,
                TotalResultCount = count,
                Results = results
            };
        }
    }
}