using CautionaryAlertsListener.Domain;
using CautionaryAlertsListener.Infrastructure;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CautionaryAlertsListener.Gateway.Interfaces
{
    public interface ICautionaryAlertGateway
    {
        Task<ICollection<PropertyAlertNew>> GetEntitiesByMMHAndTenureAsync(string mmhId, string tenureId = null);       
        Task UpdateEntityAsync(PropertyAlertNew entity);
        Task UpdateEntitiesAsync(IEnumerable<PropertyAlertNew> propertyAlerts);
        Task DeleteEntityAsync(PropertyAlertNew entity);
    }
}
