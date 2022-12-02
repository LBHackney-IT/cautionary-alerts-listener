using CautionaryAlertsListener.Domain;
using CautionaryAlertsListener.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CautionaryAlertsListener.Gateway.Interfaces
{
    public interface ICautionaryAlertGateway
    {
        Task<ICollection<PropertyAlert>> GetEntitiesByMMHAndTenureAsync(string mmhId, string tenureId = null);       
        Task UpdateEntityAsync(PropertyAlert entity);
        Task UpdateEntitiesAsync(IEnumerable<PropertyAlert> propertyAlerts);
    }
}
