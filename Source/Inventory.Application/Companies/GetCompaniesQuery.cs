using Inventory.Application.Interfaces;
using Inventory.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.Companies;

public class GetCompaniesQuery : PaginatedRequestBaseModel, IRequest<PaginatedResponseBaseModel<CompanyDto>>
{
    public long? Prefix { get; set; }
    public string? Name { get; set; }

    private sealed class
        GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, PaginatedResponseBaseModel<CompanyDto>>
    {
        private readonly IInventoryDbContext _db;

        public GetCompaniesQueryHandler(IInventoryDbContext db)
        {
            _db = db;
        }

        public async Task<PaginatedResponseBaseModel<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
        {
            var query = _db.Companies.AsNoTracking().AsQueryable();
            
            if (request.Prefix.HasValue)
            {
                query = query.Where(x => x.Prefix == request.Prefix.Value);
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                query = query.Where(x => x.Name.ToLower().Contains(request.Name.ToLower()));
            }

            var count = await query.CountAsync(cancellationToken);
            var results = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize)
                .Select(x => new CompanyDto()
                {
                    Name = x.Name,
                    Prefix = x.Prefix,
                    Id = x.Id
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