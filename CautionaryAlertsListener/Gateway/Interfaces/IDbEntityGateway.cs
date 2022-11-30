using CautionaryAlertsListener.Domain;
using System;
using System.Threading.Tasks;

namespace CautionaryAlertsListener.Gateway.Interfaces
{
    public interface IDbEntityGateway
    {
        Task<DomainEntity> GetEntityAsync(Guid id);
        Task SaveEntityAsync(DomainEntity entity);
    }
}
