using Hackney.Shared.CautionaryAlerts.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CautionaryAlertsListener.Gateway.Interfaces
{
    public interface ICautionaryAlertGateway
    {
        Task<ICollection<PropertyAlertNew>> GetEntitiesByMMHIdAndPropertyReferenceAsync(string mmhId, string propertyReference = null);
        Task SaveEntitiesAsync(IEnumerable<PropertyAlertNew> entities);
        Task UpdateEntityAsync(PropertyAlertNew entity);
        Task UpdateEntitiesAsync(IEnumerable<PropertyAlertNew> propertyAlerts);
    }
}
