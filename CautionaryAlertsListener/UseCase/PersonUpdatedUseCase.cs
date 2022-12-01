using CautionaryAlertsListener.Boundary;
using CautionaryAlertsListener.Domain;
using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.Infrastructure.Exceptions;
using CautionaryAlertsListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using System.Threading.Tasks;
using System;
using CautionaryAlertsListener.Infrastructure;
using System.Collections;
using System.Collections.Generic;

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

            var entityCollection = await _gateway.GetEntitiesByMMHAsync(message.EntityId.ToString()).ConfigureAwait(false);
            if (entityCollection is null) throw new EntityNotFoundException<PropertyAlert>(message.EntityId);

            var objectProps = message.EventData.NewData.GetType().GetProperties();
            var collectionToUpdate = new List<PropertyAlert>();
            foreach (var entity in entityCollection)
            {
                foreach (var property in objectProps)
                {
                    var newValue = property.GetValue(message.EventData.NewData).ToString();
                    var oldValue = property.GetValue(message.EventData.OldData).ToString();
                    entity.PersonName = entity.PersonName.Replace(oldValue, newValue);
                }

                collectionToUpdate.Add(entity);
            }

            await _gateway.UpdateEntitiesAsync(collectionToUpdate).ConfigureAwait(false);
        }
    }
}
