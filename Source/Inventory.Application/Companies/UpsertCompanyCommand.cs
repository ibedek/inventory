using Inventory.Application.Interfaces;
using Inventory.Application.Models;
using Inventory.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Companies;

public class UpsertCompanyCommand : IRequest<ResponseBaseModel<CompanyDto>>
{
    public Guid? Id { get; set; }
    public long Prefix { get; set; }
    public string Name { get; set; } = default!;

    private sealed class UpsertCompanyCommandHandler : IRequestHandler<UpsertCompanyCommand, ResponseBaseModel<CompanyDto>>
    {
        private readonly IInventoryDbContext _db;
        private readonly ILogger<UpsertCompanyCommandHandler> _logger;
        private readonly IDateTime _dateTime;
        private readonly ICurrentUser _currentUser;

        public UpsertCompanyCommandHandler(IInventoryDbContext db, ILogger<UpsertCompanyCommandHandler> logger, IDateTime dateTime, ICurrentUser currentUser)
        {
            _db = db;
            _logger = logger;
            _dateTime = dateTime;
            _currentUser = currentUser;
        }

        public async Task<ResponseBaseModel<CompanyDto>> Handle(UpsertCompanyCommand request, CancellationToken cancellationToken)
        {
            Company? company;
            string[] errors = new string[0] { };
            if (string.IsNullOrEmpty(request.Name))
            {
                errors.Append("NAME_REQUIRED");
            }

            if (request.Prefix <= 0)
            {
                errors.Append("PREFIX_REQUIRED");
            }

            if (errors.Length > 0)
            {
                return ResponseBaseModel<CompanyDto>.Failure(errors);
            }

            if (request.Id.HasValue)
            {
                company = await _db.Companies.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
                if (company is null)
                {
                    company = new(_dateTime.Now, _currentUser.Username);
                }
                if (!company.Name.Equals(request.Name))
                {
                    _logger.LogInformation("Found Company with different name, updating");
                    company.Name = request.Name;
                    company.UpdatedAt = _dateTime.Now;
                    company.UpdatedBy = _currentUser.Username;
                    _db.Companies.Update(company);
                }
            }
            else
            {
                _logger.LogInformation("ID not provided, trying to find company by prefix");
                company = await _db.Companies.FirstOrDefaultAsync(x => x.Prefix == request.Prefix, cancellationToken);
                if (company != null && !company.Name.Equals(request.Name))
                {
                    _logger.LogInformation("Found company by prefix, but with different name, updating");
                    company.Name = request.Name;
                    company.UpdatedAt = _dateTime.Now;
                    company.UpdatedBy = _currentUser.Username;
                    _db.Companies.Update(company);
                }
                else
                {
                    company = new(_dateTime.Now, _currentUser.Username)
                    {
                        Id = Guid.NewGuid(),
                        Name = request.Name,
                        Prefix = request.Prefix,
                    };
                    _db.Companies.Add(company);
                }
            }

            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                return ResponseBaseModel<CompanyDto>.Succeeded(new()
                {
                    Id = company.Id,
                    Name = company.Name,
                    Prefix = company.Prefix
                });
            }
            catch (Exception e)
            {
                _logger.LogError("An error occured while saving changes: {Message}; StackTrace: {StackTrace}", e.Message, e.StackTrace);
                return ResponseBaseModel<CompanyDto>.Failure(new[] { "UNKNOWN_ERROR" });
            }
        }
    }
}
