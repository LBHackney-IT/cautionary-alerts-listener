using CautionaryAlertsListener.Boundary;
using CautionaryAlertsListener.Domain;
using CautionaryAlertsListener.Factories;
using CautionaryAlertsListener.Gateway;
using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.Infrastructure;
using CautionaryAlertsListener.Infrastructure.Exceptions;
using CautionaryAlertsListener.UseCase.Interfaces;
using Hackney.Core.Logging;
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
            var entity = (await _gateway.GetEntitiesByMMHAndTenureAsync(householdMember.Id, tenure.TenuredAsset.Id))
                .FirstOrDefault();

            if (entity is null) throw new EntityNotFoundException<PropertyAlert>(message.EntityId);

            entity.Address = tenure.TenuredAsset.FullAddress;
            entity.PropertyReference = tenure.TenuredAsset.Id;
            entity.UPRN = tenure.TenuredAsset.Uprn;

            await _gateway.UpdateEntityAsync(entity).ConfigureAwait(false);
        }

        private static HouseholdMembers GetAddedOrUpdatedHouseholdMember(EventData eventData)
        {
            var oldHms = GetHouseholdMembersFromEventData(eventData.OldData);
            var newHms = GetHouseholdMembersFromEventData(eventData.NewData);

            return newHms.Except(oldHms).FirstOrDefault();
        }

        private static List<HouseholdMembers> GetHouseholdMembersFromEventData(object data)
        {
            var dataDic = (data is Dictionary<string, object>) ? data as Dictionary<string, object> : ObjectFactory.ConvertFromObject<Dictionary<string, object>>(data);
            var hmsObj = dataDic["householdMembers"];
            return (hmsObj is List<HouseholdMembers>) ? hmsObj as List<HouseholdMembers> : ObjectFactory.ConvertFromObject<List<HouseholdMembers>>(hmsObj);
        }
    }
}
