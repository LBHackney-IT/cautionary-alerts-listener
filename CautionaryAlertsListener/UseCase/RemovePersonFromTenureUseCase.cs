using CautionaryAlertsListener.Infrastructure.Exceptions;
using CautionaryAlertsListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using System.Threading.Tasks;
using System;
using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.Infrastructure;
using System.Linq;
using Hackney.Core.Sns;
using Hackney.Shared.Tenure.Domain;

namespace CautionaryAlertsListener.UseCase
{
    public class RemovePersonFromTenureUseCase : IRemovePersonFromTenureUseCase
    {
        private readonly ICautionaryAlertGateway _gateway;
        private readonly ITenureApiGateway _tenureApiGateway;

        public RemovePersonFromTenureUseCase(ICautionaryAlertGateway gateway, ITenureApiGateway tenureApiGateway)
        {
            _gateway = gateway;
            _tenureApiGateway = tenureApiGateway;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            var tenure = await _tenureApiGateway.GetTenureByIdAsync(message.EntityId, message.CorrelationId);
            if (tenure is null) throw new EntityNotFoundException<TenureInformation>(message.EntityId);

            var householdMember = GetRemovedHouseholdMember(message.EventData);
            if (householdMember is null) throw new HouseholdMembersNotChangedException(message.EntityId, message.CorrelationId);

            var alerts = await _gateway.GetEntitiesByMMHIdAndPropertyReferenceAsync(householdMember.Id.ToString(), tenure.TenuredAsset.PropertyReference);
            if (alerts is null) return;

            foreach (var item in alerts)
            {
                item.Address = null;
                item.PropertyReference = null;
                item.UPRN = null;
            }

            await _gateway.UpdateEntitiesAsync(alerts).ConfigureAwait(false);
        }

        private static HouseholdMembers GetRemovedHouseholdMember(EventData eventData)
        {
            var oldHms = Helpers.GetHouseholdMembersFromEventData(eventData.OldData);
            var newHms = Helpers.GetHouseholdMembersFromEventData(eventData.NewData);

            return oldHms.Except(newHms).FirstOrDefault();
        }
    }
}
