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

            var householdMember = GetAddedOrUpdatedHouseholdMember(message.EventData);
            if (householdMember is null) throw new HouseholdMembersNotChangedException(message.EntityId, message.CorrelationId);

            var cautionaryAlerts = (await _gateway.GetEntitiesByMMHIdAndPropertyReferenceAsync(householdMember.Id.ToString()).ConfigureAwait(false));
            if (cautionaryAlerts is null) return;

            var tenure = await _tenureApiGateway.GetTenureByIdAsync(message.EntityId, message.CorrelationId)
                                                .ConfigureAwait(false);
            if (tenure is null) throw new EntityNotFoundException<TenureInformation>(message.EntityId);

            var newAlerts = new List<PropertyAlertNew>();
            foreach (var alert in cautionaryAlerts)
            {
                var newAlert = new PropertyAlertNew
                {
                    Address = tenure.TenuredAsset.FullAddress,
                    UPRN = tenure.TenuredAsset.Uprn,
                    PropertyReference = tenure.TenuredAsset.PropertyReference,
                    AssureReference = alert.AssureReference,
                    MMHID = alert.MMHID,
                    PersonName = alert.PersonName,
                    Code = alert.Code,
                    CautionOnSystem = alert.CautionOnSystem,
                    DateOfIncident = alert.DateOfIncident,
                    Reason = alert.Reason
                }; // copy cautionary alert to new property
                newAlerts.Add(newAlert);
            }
            await _gateway.SaveEntitiesAsync(newAlerts).ConfigureAwait(false);
        }

        private static HouseholdMembers GetAddedOrUpdatedHouseholdMember(EventData eventData)
        {
            var oldHms = Helpers.GetHouseholdMembersFromEventData(eventData.OldData);
            var newHms = Helpers.GetHouseholdMembersFromEventData(eventData.NewData);

            return newHms.Except(oldHms).FirstOrDefault();
        }
    }
}
