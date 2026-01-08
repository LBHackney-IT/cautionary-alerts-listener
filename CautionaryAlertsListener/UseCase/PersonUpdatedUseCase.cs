using CautionaryAlertsListener.Gateway.Interfaces;
using CautionaryAlertsListener.UseCase.Interfaces;
using Hackney.Core.Logging;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Hackney.Core.Sns;
using System.Linq;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
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

            var cautionaryAlerts = await _gateway.GetEntitiesByMMHIdAndPropertyReferenceAsync(message.EntityId.ToString()).ConfigureAwait(false);
            if (cautionaryAlerts is null || !cautionaryAlerts.Any()) return;

            var deserializedNewData = JObject.Parse(message.EventData.NewData.ToString());
            var deresializedOldData = JObject.Parse(message.EventData.OldData.ToString());
            var propCollection = deserializedNewData.Properties().ToList();

            var collectionToUpdate = new List<PropertyAlertNew>();
            foreach (var entity in cautionaryAlerts)
            {
                entity.PersonName = $"{deserializedNewData["FirstName"]} {deserializedNewData["LastName"]}";

                collectionToUpdate.Add(entity);
            }

            await _gateway.UpdateEntitiesAsync(collectionToUpdate).ConfigureAwait(false);
        }
    }
}
