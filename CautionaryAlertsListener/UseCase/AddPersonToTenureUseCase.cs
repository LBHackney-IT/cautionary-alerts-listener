using CautionaryAlertsListener.Boundary;
using CautionaryAlertsListener.Domain;
using CautionaryAlertsListener.Factories;
using CautionaryAlertsListener.Gateway;
using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.Infrastructure;
using CautionaryAlertsListener.Infrastructure.Exceptions;
using CautionaryAlertsListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using Hackney.Core.Sns;
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

            var tenure = await _tenureApiGateway.GetTenureByIdAsync(message.EntityId, message.CorrelationId)
                                                .ConfigureAwait(false);
            if (tenure is null) throw new EntityNotFoundException<TenureInformation>(message.EntityId);

            var householdMember = GetAddedOrUpdatedHouseholdMember(message.EventData);
            if (householdMember is null) throw new HouseholdMembersNotChangedException(message.EntityId, message.CorrelationId);

            var entity = (await _gateway.GetEntitiesByMMHAndTenureAsync(householdMember.Id, tenure.Id).ConfigureAwait(false))?.FirstOrDefault();

            if (entity is null) return;

            entity.Address = tenure.TenuredAsset.FullAddress;
            entity.PropertyReference = tenure.Id;
            entity.UPRN = tenure.TenuredAsset.Uprn;

            await _gateway.UpdateEntityAsync(entity).ConfigureAwait(false);
        }

        private static HouseholdMembers GetAddedOrUpdatedHouseholdMember(EventData eventData)
        {
            var oldHms = Helpers.GetHouseholdMembersFromEventData(eventData.OldData);
            var newHms = Helpers.GetHouseholdMembersFromEventData(eventData.NewData);

            return newHms.Except(oldHms).FirstOrDefault();
        }
    }
}
