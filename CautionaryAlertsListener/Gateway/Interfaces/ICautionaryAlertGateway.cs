using CautionaryAlertsListener.Domain;
using CautionaryAlertsListener.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CautionaryAlertsListener.Gateway.Interfaces
{
    public interface ICautionaryAlertGateway
    {
        Task<ICollection<PropertyAlert>> GetEntitiesByMMHAsync(string mmhId);
        Task<PropertyAlert> GetEntityByTenureAndNameAsync(string tenureId, string personName);        
        Task UpdateEntityAsync(PropertyAlert entity);
        Task UpdateEntitiesAsync(IEnumerable<PropertyAlert> propertyAlerts);
    }
}
