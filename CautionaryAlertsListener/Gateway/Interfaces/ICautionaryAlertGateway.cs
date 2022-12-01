using CautionaryAlertsListener.Domain;
using CautionaryAlertsListener.Infrastructure;
using System;
using System.Threading.Tasks;

namespace CautionaryAlertsListener.Gateway.Interfaces
{
    public interface ICautionaryAlertGateway
    {
        Task<PropertyAlert> GetEntityByMMHAsync(string mmhId);
        Task<PropertyAlert> GetEntityByTenureAndNameAsync(string mmhId);
        Task UpdateEntityAsync(PropertyAlert entity);
    }
}
