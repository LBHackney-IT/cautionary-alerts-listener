using CautionaryAlertsListener.Domain;
using CautionaryAlertsListener.Factories;
using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.Infrastructure;
using Hackney.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CautionaryAlertsListener.Gateway
{
    public class CautionaryAlertGateway : ICautionaryAlertGateway
    {
        private readonly CautionaryAlertContext _cautionaryAlertDbContext;
        private readonly ILogger<CautionaryAlertGateway> _logger;

        public CautionaryAlertGateway(CautionaryAlertContext cautionaryAlertDbContext, ILogger<CautionaryAlertGateway> logger)
        {
            _logger = logger;
            _cautionaryAlertDbContext = cautionaryAlertDbContext;
        }

        [LogCall]
        public async Task<PropertyAlert> GetEntityByMMHAsync(string mmhId)
        {
            _logger.LogDebug($"Calling Postgres for mmhId {mmhId}");
            var dbEntity = await _cautionaryAlertDbContext.PropertyAlerts
                .FirstOrDefaultAsync(x => x.MMHID == mmhId).ConfigureAwait(false);
            return dbEntity;
        }

        [LogCall]
        public async Task<PropertyAlert> GetEntityByTenureAndNameAsync(string tenureId, string personName)
        {
            _logger.LogDebug($"Calling Postgres for tenureId {tenureId} and name {personName}");
            var dbEntity = await _cautionaryAlertDbContext.PropertyAlerts
                .FirstOrDefaultAsync(x => x.PropertyReference == tenureId && x.PersonName == personName)
                .ConfigureAwait(false);
            return dbEntity;
        }

        [LogCall]
        public async Task UpdateEntityAsync(PropertyAlert entity)
        {
            _logger.LogDebug($"Calling Postgres.SaveAsync for mmhId {entity.MMHID}");
            _cautionaryAlertDbContext.PropertyAlerts.Update(entity);
            await _cautionaryAlertDbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
