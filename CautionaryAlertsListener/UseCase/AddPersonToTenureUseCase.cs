using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.Infrastructure;
using CautionaryAlertsListener.Infrastructure.Exceptions;
using CautionaryAlertsListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using Hackney.Core.Sns;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using Hackney.Shared.Tenure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CautionaryAlertsListener.UseCase
{
    public class AddPersonToTenureUseCase : IAddPersonToTenureUseCase
    {
        private readonly ICautionaryAlertGateway _gateway;
        private readonly ITenureApiGateway _tenureApiGateway;

        public AddPersonToTenureUseCase(ICautionaryAlertGateway gateway, ITenureApiGateway tenureApiGateway)
        {
            _gateway = gateway;
            _tenureApiGateway = tenureApiGateway;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            var householdMember = GetAddedOrUpdatedHouseholdMember(message);
            if (householdMember is null) throw new HouseholdMembersNotChangedException(message.EntityId, message.CorrelationId);

            var cautionaryAlerts = (await _gateway.GetEntitiesByMMHIdAndPropertyReferenceAsync(householdMember.Id.ToString()).ConfigureAwait(false));
            if (cautionaryAlerts is null || !cautionaryAlerts.Any()) return;

            var tenure = await _tenureApiGateway.GetTenureByIdAsync(message.EntityId, message.CorrelationId)
                                                .ConfigureAwait(false);
            if (tenure is null) throw new EntityNotFoundException<TenureInformation>(message.EntityId);

            var alert = cautionaryAlerts.FirstOrDefault();
            alert.PropertyReference = tenure.TenuredAsset.PropertyReference;
            alert.UPRN = tenure.TenuredAsset.Uprn;
            alert.Address = tenure.TenuredAsset.FullAddress;

            await _gateway.UpdateEntityAsync(alert).ConfigureAwait(false);
        }

        private static HouseholdMembers GetAddedOrUpdatedHouseholdMember(EntityEventSns message)
        {
            try
            {
                var oldHms = Helpers.GetHouseholdMembersFromEventData(message.EventData.OldData);
                var newHms = Helpers.GetHouseholdMembersFromEventData(message.EventData.NewData);
                return newHms.Except(oldHms).FirstOrDefault();
            }
            catch (HouseholdMembersNotChangedException)
            {
                throw new HouseholdMembersNotChangedException(message.EntityId, message.CorrelationId);
            }
        }
    }
}
