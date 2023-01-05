using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.Infrastructure;
using Hackney.Core.Logging;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
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
        public async Task<ICollection<PropertyAlertNew>> GetEntitiesByMMHIdAndPropertyReferenceAsync(string mmhId, string propertyReference = null)
        {
            if (string.IsNullOrEmpty(mmhId))
            {
                throw new ArgumentNullException(nameof(mmhId));
            }

            _logger.LogDebug($"Calling Postgres for mmhId {mmhId}");
            var query = _cautionaryAlertDbContext.PropertyAlerts
                .Where(x => x.MMHID == mmhId);

            if (!string.IsNullOrWhiteSpace(propertyReference))
            {
                query = query.Where(x => x.PropertyReference == propertyReference);
            }

            return await query
                .ToListAsync()
                .ConfigureAwait(false);
        }

        [LogCall]
        public async Task UpdateEntityAsync(PropertyAlertNew entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _logger.LogDebug($"Calling Postgres.SaveAsync for mmhId {entity.MMHID}");
            _cautionaryAlertDbContext.PropertyAlerts.Update(entity);
            await _cautionaryAlertDbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        [LogCall]
        public async Task UpdateEntitiesAsync(IEnumerable<PropertyAlertNew> propertyAlerts)
        {
            if (propertyAlerts is null || !propertyAlerts.Any())
            {
                throw new ArgumentNullException(nameof(propertyAlerts));
            }

            _logger.LogDebug($"Calling Postgres.SaveAsync for mmhId {string.Join(',', propertyAlerts.Select(x => x.MMHID))}");

            foreach (var propertyAlert in propertyAlerts)
                _cautionaryAlertDbContext.PropertyAlerts.Update(propertyAlert);

            await _cautionaryAlertDbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        [LogCall]
        public async Task SaveEntitiesAsync(IEnumerable<PropertyAlertNew> entities)
        {
            _logger.LogDebug($"Calling Postgress.SaveAsync");

            _cautionaryAlertDbContext.PropertyAlerts.AddRange(entities);
            await _cautionaryAlertDbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
