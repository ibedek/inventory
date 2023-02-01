using Inventory.Application.Companies;
using Inventory.Application.Interfaces;
using Inventory.Application.Models;
using Inventory.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Products;

public class InsertProductCommand : IRequest<ResponseBaseModel<ProductDto>>
{
    public string ItemName { get; set; } = default!;
    public long ItemReference { get; set; }
    public string CompanyName { get; set; } = default!;
    public long CompanyPrefix { get; set; }

    private sealed class
        InsertProductCommandHandler : IRequestHandler<InsertProductCommand, ResponseBaseModel<ProductDto>>
    {
        private readonly IInventoryDbContext _db;
        private readonly ILogger<InsertProductCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IDateTime _dateTime;
        private readonly ICurrentUser _currentUser;

        public InsertProductCommandHandler(IInventoryDbContext db, ILogger<InsertProductCommandHandler> logger,
            IMediator mediator, IDateTime dateTime, ICurrentUser currentUser)
        {
            _db = db;
            _logger = logger;
            _mediator = mediator;
            _dateTime = dateTime;
            _currentUser = currentUser;
        }

        public async Task<ResponseBaseModel<ProductDto>> Handle(InsertProductCommand request,
            CancellationToken cancellationToken)
        {
            List<string> errors = new();
            Company? company = null;
            Product? product = null;
            if (request.CompanyPrefix <= 0)
            {
                errors.Add("INVALID_COMPANY_PREFIX");
            }
            else
            {
                company = await _db.Companies.AsNoTracking().Where(x => x.Prefix == request.CompanyPrefix)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (company is null && string.IsNullOrEmpty(request.CompanyName))
            {
                errors.Add("INVALID_COMPANY_NAME");
            }

            if (request.ItemReference <= 0)
            {
                errors.Add("INVALID_ITEM_REFERENCE");
            }
            else
            {
                product = await _db.Products.Where(x => x.ReferenceNumber == request.ItemReference)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (product is null && string.IsNullOrEmpty(request.ItemName))
            {
                errors.Add("INVALID_PRODUCT_NAME");
            }

            if (errors.Count > 0)
            {
                return ResponseBaseModel<ProductDto>.Failure(errors.ToArray());
            }

            if (company is null)
            {
                var companyUpsertResult = await _mediator.Send(new UpsertCompanyCommand
                {
                    Prefix = request.CompanyPrefix,
                    Name = request.CompanyName
                }, cancellationToken);
                if (!companyUpsertResult.Succeess)
                {
                    return ResponseBaseModel<ProductDto>.Failure(companyUpsertResult.Errors);
                }

                company = new()
                {
                    Id = companyUpsertResult.Result.Id,
                    Prefix = companyUpsertResult.Result.Prefix,
                    Name = companyUpsertResult.Result.Name
                };
            }

            if (product is null)
            {
                product = new(_dateTime.Now, _currentUser.Username)
                {
                    Name = request.ItemName,
                    ReferenceNumber = request.ItemReference,
                    CompanyId = company.Id
                };
                _db.Products.Add(product);
            }

            try
            {
                await _db.SaveChangesAsync(cancellationToken);

                return ResponseBaseModel<ProductDto>.Succeeded(new()
                {
                    Id = product.Id,
                    CompanyName = company.Name,
                    Name = product.Name,
                    CompanyPrefix = company.Prefix,
                    ItemReference = product.ReferenceNumber,
                });
            }
            catch (Exception e)
            {
                _logger.LogError("An error occured while saving product: {Message}; StackTrace: {StackTrace}",
                    e.Message, e.StackTrace);
                return ResponseBaseModel<ProductDto>.Failure(new[] { "UNKNOWN_ERROR" });
            }
        }
    }
}