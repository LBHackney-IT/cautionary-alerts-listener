using System.Threading.Tasks;
using System;
using Hackney.Shared.Tenure.Domain;

namespace CautionaryAlertsListener.Gateway.Interfaces
{
    public interface ITenureApiGateway
    {
        Task<TenureInformation> GetTenureByIdAsync(Guid id, Guid correlationId);
    }
}
