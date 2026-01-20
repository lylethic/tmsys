using server.Common.Interfaces;
using server.Domain.Entities;

namespace server.Application.Common.Interfaces;

public interface ICompanyGeofence : IRepository<CompanyGeofence>
{
    Task<CompanyGeofence?> GetActiveAsync();
}
