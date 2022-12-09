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
            return _fixture.Build<EntityEventSns>()
                           .With(x => x.EntityId, eventId)
                           .With(x => x.EventType, _eventType)
                           .With(x => x.CorrelationId, _correlationId)
                           .With(x => x.EventData, EventDataFixture.CreatePersonUpdateData())
                           .Create();
        }

        public void ThenThePersonNameIsUpdated(PropertyAlertNew originalCautionaryAlertDb, CautionaryAlertContext dbContext)
        {
            var updatedCautionaryAlertInDb = dbContext.PropertyAlerts.AsNoTracking().FirstOrDefault(x=> x.Id == originalCautionaryAlertDb.Id);
            updatedCautionaryAlertInDb.Should().BeEquivalentTo(originalCautionaryAlertDb, config => config.Excluding(y => y.PersonName));
            updatedCautionaryAlertInDb.PersonName.Should().StartWith(FixtureConstants.NewFirstName);
            updatedCautionaryAlertInDb.PersonName.Should().EndWith(FixtureConstants.NewLastName);
        }
    }
}
