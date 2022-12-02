using Amazon.DynamoDBv2.Model;
using Amazon.XRay.Recorder.Core.Internal.Entities;
using CautionaryAlertsListener.Domain;
using CautionaryAlertsListener.Factories;
using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.Infrastructure;
using Hackney.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        public async Task<ICollection<PropertyAlert>> GetEntitiesByMMHAndTenureAsync(string mmhId, string tenureId = null)
        {
            _logger.LogDebug($"Calling Postgres for mmhId {mmhId}");
            var query = _cautionaryAlertDbContext.PropertyAlerts
                .Where(x => x.MMHID == mmhId);

            if (!string.IsNullOrWhiteSpace(tenureId))
            {
                query = query.Where(x => x.PropertyReference == tenureId);
            }

            return await query
                .ToListAsync()
                .ConfigureAwait(false);
        }

        [LogCall]
        public async Task UpdateEntityAsync(PropertyAlert entity)
        {
            _logger.LogDebug($"Calling Postgres.SaveAsync for mmhId {entity.MMHID}");
            _cautionaryAlertDbContext.PropertyAlerts.Update(entity);
            await _cautionaryAlertDbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        [LogCall]
        public async Task UpdateEntitiesAsync(IEnumerable<PropertyAlert> propertyAlerts)
        {
            _logger.LogDebug($"Calling Postgres.SaveAsync for mmhId {string.Join(',', propertyAlerts.Select(x => x.MMHID))}");

            foreach (var propertyAlert in propertyAlerts)
                _cautionaryAlertDbContext.PropertyAlerts.Update(propertyAlert);

            await _cautionaryAlertDbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        [LogCall]
        public async Task DeleteEntityAsync(PropertyAlert entity)
        {
            _logger.LogDebug($"Deleting Postgres entity for mmhId {entity.MMHID} with tenureId {entity.PropertyReference}");
            _cautionaryAlertDbContext.PropertyAlerts.Remove(entity);
            await _cautionaryAlertDbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
