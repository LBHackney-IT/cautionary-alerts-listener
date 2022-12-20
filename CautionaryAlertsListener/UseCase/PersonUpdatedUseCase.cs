using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.Infrastructure.Exceptions;
using CautionaryAlertsListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using System.Threading.Tasks;
using System;
using CautionaryAlertsListener.Infrastructure;
using System.Collections.Generic;
using Hackney.Core.Sns;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            var entityCollection = await _gateway.GetEntitiesByMMHAndPropertyReferenceAsync(message.EntityId.ToString()).ConfigureAwait(false);
            if (entityCollection is null || !entityCollection.Any()) return;

            var deserializedNewData = JObject.Parse(message.EventData.NewData.ToString());
            var deresializedOldData = JObject.Parse(message.EventData.OldData.ToString());
            var propCollection = deserializedNewData.Properties().ToList();

            var collectionToUpdate = new List<PropertyAlertNew>();
            foreach (var entity in entityCollection)
            {
                foreach (var property in propCollection)
                {
                    var newValue = property.Value.ToString();
                    var oldValue = deresializedOldData[property.Name]?.ToString();
                    if (!newValue.Equals(oldValue) || !(oldValue is null))
                    {
                        entity.PersonName = entity.PersonName.Replace(oldValue, newValue);
                    }
                }

                collectionToUpdate.Add(entity);
            }

            await _gateway.UpdateEntitiesAsync(collectionToUpdate).ConfigureAwait(false);
        }
    }
}
