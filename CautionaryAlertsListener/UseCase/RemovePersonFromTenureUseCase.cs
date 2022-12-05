using CautionaryAlertsListener.Boundary;
using CautionaryAlertsListener.Domain;
using CautionaryAlertsListener.Infrastructure.Exceptions;
using CautionaryAlertsListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using System.Threading.Tasks;
using System;
using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.Infrastructure;
using System.Linq;
using Hackney.Core.Sns;

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
            var entity = await _gateway.GetEntitiesByMMHAndTenureAsync(householdMember.Id, tenure.Id);
            await _gateway.DeleteEntityAsync(entity.FirstOrDefault());
        }

        private static HouseholdMembers GetRemovedHouseholdMember(EventData eventData)
        {
            var oldHms = Helpers.GetHouseholdMembersFromEventData(eventData.OldData);
            var newHms = Helpers.GetHouseholdMembersFromEventData(eventData.NewData);

            return oldHms.Except(newHms).FirstOrDefault();
        }
    }
}
