using System.Threading.Tasks;
using System;
using CautionaryAlertsListener.Domain;

namespace CautionaryAlertsListener.Gateway.Interfaces
{
    public interface ITenureApiGateway
    {
        Task<TenureInformation> GetTenureByIdAsync(Guid id, Guid correlationId);
    }
}
