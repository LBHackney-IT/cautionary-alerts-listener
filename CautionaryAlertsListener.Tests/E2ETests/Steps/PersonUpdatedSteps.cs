using Amazon.Lambda.SQSEvents;
using AutoFixture;
using CautionaryAlertsListener.Infrastructure;
using FluentAssertions;
using Hackney.Core.Sns;
using Hackney.Shared.CautionaryAlerts.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace CautionaryAlertsListener.Tests.E2ETests.Steps
{
    public class PersonUpdatedSteps : BaseSteps
    {
        public const string NewFirstName = "NewFirstName";
        public const string NewLastName = "NewLastName";

        public PersonUpdatedSteps()
        {
            _eventType = EventTypes.PersonUpdatedEvent;
        }

        public async Task WhenTheFunctionIsTriggered(Guid id)
        {
            await TriggerFunction(id).ConfigureAwait(false);
        }

        public async Task WhenTheFunctionIsTriggered(SQSEvent.SQSMessage message)
        {
            await TriggerFunction(message).ConfigureAwait(false);
        }

        public void ThenNoExceptionIsThrown()
        {
            _lastException.Should().BeNull();
        }

        public void ThenNothingShouldBeDone()
        { }

        protected override EntityEventSns CreateEvent(Guid eventId, string eventType)
        {
            dynamic oldData = new ExpandoObject();
            oldData.firstName = CreateCautionaryAlertFixture.OldFirstName;
            oldData.lastName = CreateCautionaryAlertFixture.OldLastName;

            dynamic newData = new ExpandoObject();
            newData.firstName = NewFirstName;
            newData.lastName = NewLastName;

            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EntityId, eventId)
                           .With(x => x.EventType, _eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .With(x => x.EventData, new EventData()
                           {
                               OldData = oldData,
                               NewData = newData
                           })
                           .Create();
        }

        public void ThenThePersonNameIsUpdated(PropertyAlertNew originalCautionaryAlertDb, CautionaryAlertContext dbContext)
        {
            var updatedCautionaryAlertInDb = dbContext.PropertyAlerts.AsNoTracking().FirstOrDefault(x=> x.Id == originalCautionaryAlertDb.Id);
            updatedCautionaryAlertInDb.Should().BeEquivalentTo(originalCautionaryAlertDb, config => config.Excluding(y => y.PersonName));
            updatedCautionaryAlertInDb.PersonName.Should().StartWith(NewFirstName);
            updatedCautionaryAlertInDb.PersonName.Should().EndWith(NewLastName);
        }
    }
}
