using CautionaryAlertsListener.Boundary;
using CautionaryAlertsListener.Domain;
using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.Infrastructure.Exceptions;
using CautionaryAlertsListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using System.Threading.Tasks;
using System;
using CautionaryAlertsListener.Infrastructure;

namespace CautionaryAlertsListener.UseCase
{
    public class PersonUpdatedUseCase : IPersonUpdatedUseCase
    {
        private readonly ICautionaryAlertGateway _gateway;

        public PersonUpdatedUseCase(ICautionaryAlertGateway gateway)
        {
            _gateway = gateway;
        }

        [LogCall]
        public async Task ProcessMessageAsync(EntityEventSns message)
        {
            if (message is null) throw new ArgumentNullException(nameof(message));

            var entity = await _gateway.GetEntityByMMHAsync(message.EntityId.ToString()).ConfigureAwait(false);
            if (entity is null) throw new EntityNotFoundException<PropertyAlert>(message.EntityId);

            var objectProps = message.EventData.NewData.GetType().GetProperties();

            foreach (var property in objectProps)
            {
                var newValue = property.GetValue(message.EventData.NewData).ToString();
                var oldValue = property.GetValue(message.EventData.OldData).ToString();
                entity.PersonName = entity.PersonName.Replace(oldValue, newValue);
            }

            await _gateway.UpdateEntityAsync(entity).ConfigureAwait(false);
        }
    }
}
